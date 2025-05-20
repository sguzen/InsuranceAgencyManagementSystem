using IAMS.Application.DTOs.Identity;
using IAMS.Application.Models;
using IAMS.Identity.Models;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityService _identityService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IIdentityService identityService,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _identityService = identityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserQueryParams queryParams)
        {
            var query = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                query = query.Where(u =>
                    u.Email.Contains(queryParams.SearchTerm) ||
                    u.FirstName.Contains(queryParams.SearchTerm) ||
                    u.LastName.Contains(queryParams.SearchTerm));
            }

            // Apply sorting
            query = queryParams.SortBy?.ToLower() switch
            {
                "name" => queryParams.SortDirection == "desc"
                    ? query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "email" => queryParams.SortDirection == "desc"
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "created" => queryParams.SortDirection == "desc"
                    ? query.OrderByDescending(u => u.CreatedOn)
                    : query.OrderBy(u => u.CreatedOn),
                _ => query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            };

            // Calculate pagination
            var totalCount = await query.CountAsync();
            var pageSize = queryParams.PageSize > 0 ? queryParams.PageSize : 10;
            var pageNumber = queryParams.PageNumber > 0 ? queryParams.PageNumber : 1;
            var skip = (pageNumber - 1) * pageSize;

            // Get paginated results
            var users = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedOn = user.CreatedOn,
                    LastLogin = user.LastLogin,
                    Roles = roles.ToList()
                });
            }

            return Ok(new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDetailsDto>> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn,
                LastLogin = user.LastLogin,
                Roles = roles.ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createDto)
        {
            var result = await _identityService.RegisterUserAsync(new RegisterUserDto
            {
                Email = createDto.Email,
                Password = createDto.Password,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                RoleName = createDto.RoleName
            });

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetUserById), new { userId = result.UserId }, new { Id = result.UserId });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update basic properties
            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.IsActive = updateDto.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Update roles if provided
            if (updateDto.Roles != null)
            {
                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove roles not in the new list
                var rolesToRemove = currentRoles.Except(updateDto.Roles).ToList();
                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                // Add roles that are in the new list but not current
                var rolesToAdd = updateDto.Roles.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }
            }

            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Consider soft delete instead of hard delete
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        [HttpPost("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(string userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // For admin reset, no need to verify old password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Invalidate refresh tokens
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost("change-my-password")]
        [Authorize]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify old password
            if (!await _userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword))
            {
                return BadRequest(new { Error = "Current password is incorrect" });
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Invalidate refresh tokens
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}
