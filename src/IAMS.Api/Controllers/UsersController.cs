// IAMS.Api/Controllers/UsersController.cs
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Identity;
using IAMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Api.Controllers
{
    /// <summary>
    /// User administration for the current tenant. Every action returns the shared
    /// <see cref="Result"/> / <see cref="Result{T}"/> envelope, which is what
    /// IAMS.Web's <c>UsersApiClient</c> deserializes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>GET api/users?page=&amp;pageSize=&amp;search= — paged list.</summary>
        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<UserDto>>>> GetUsers(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                page = Math.Max(page, 1);
                pageSize = Math.Clamp(pageSize, 1, 200);

                var query = ApplySearch(_userManager.Users, search);
                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.Email)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var paged = new PagedResult<UserDto>(await ToDtosAsync(users), totalCount, page, pageSize);
                return Ok(Result<PagedResult<UserDto>>.Success(paged));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, Result<PagedResult<UserDto>>.InternalError("Error retrieving users"));
            }
        }

        /// <summary>GET api/users/all — every user, unpaged (used by the Web user management page).</summary>
        [HttpGet("all")]
        public async Task<ActionResult<Result<List<UserDto>>>> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
                return Ok(Result<List<UserDto>>.Success(await ToDtosAsync(users)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, Result<List<UserDto>>.InternalError("Error retrieving users"));
            }
        }

        /// <summary>GET api/users/roles — names of all assignable roles.</summary>
        [HttpGet("roles")]
        public async Task<ActionResult<Result<List<string>>>> GetAllRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Where(r => r.Name != null)
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name!)
                    .ToListAsync();
                return Ok(Result<List<string>>.Success(roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, Result<List<string>>.InternalError("Error retrieving roles"));
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<UserDto>>> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result<UserDto>.NotFound("User not found"));

                return Ok(Result<UserDto>.Success(await ToDtoAsync(user)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, Result<UserDto>.InternalError("Error retrieving user"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateUserDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(Result<UserDto>.ValidationFailure("Email and password are required",
                        new List<string> { "Email and password are required" }));

                if (!string.IsNullOrEmpty(request.ConfirmPassword) && request.ConfirmPassword != request.Password)
                    return BadRequest(Result<UserDto>.ValidationFailure("Passwords do not match",
                        new List<string> { "Passwords do not match" }));

                var roleError = await ValidateRolesAsync(request.Roles);
                if (roleError != null)
                    return BadRequest(Result<UserDto>.ValidationFailure(roleError, new List<string> { roleError }));

                if (await _userManager.FindByEmailAsync(request.Email) != null)
                    return Conflict(Result<UserDto>.Conflict("A user with this email already exists"));

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = request.IsActive
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return BadRequest(Result<UserDto>.Failure("Could not create user", Describe(result)));

                if (request.Roles.Count > 0)
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                    if (!roleResult.Succeeded)
                        return BadRequest(Result<UserDto>.Failure("User created but roles could not be assigned", Describe(roleResult)));
                }

                _logger.LogInformation("User {Email} created", user.Email);
                return Ok(Result<UserDto>.Success(await ToDtoAsync(user), "User created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, Result<UserDto>.InternalError("Error creating user"));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Result>> UpdateUser(string id, [FromBody] UpdateUserDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result.NotFound("User not found"));

                var roleError = await ValidateRolesAsync(request.Roles);
                if (roleError != null)
                    return BadRequest(Result.ValidationFailure(roleError, new List<string> { roleError }));

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.IsActive = request.IsActive;

                if (!string.IsNullOrWhiteSpace(request.Email) &&
                    !string.Equals(request.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    // UserManager.UpdateAsync validates uniqueness (RequireUniqueEmail).
                    user.Email = request.Email;
                    user.UserName = request.Email;
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(Result.Failure("Could not update user", Describe(result)));

                var roleSync = await SyncRolesAsync(user, request.Roles);
                if (roleSync != null)
                    return BadRequest(Result.Failure("User updated but roles could not be changed", roleSync));

                return Ok(Result.Success("User updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, Result.InternalError("Error updating user"));
            }
        }

        /// <summary>DELETE api/users/{id} — soft delete: the account is deactivated, not removed.</summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Result>> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result.NotFound("User not found"));

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(Result.Failure("Could not deactivate user", Describe(result)));

                return Ok(Result.Success("User deactivated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, Result.InternalError("Error deleting user"));
            }
        }

        [HttpGet("{id:guid}/roles")]
        public async Task<ActionResult<Result<List<string>>>> GetUserRoles(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result<List<string>>.NotFound("User not found"));

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(Result<List<string>>.Success(roles.ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", id);
                return StatusCode(500, Result<List<string>>.InternalError("Error retrieving user roles"));
            }
        }

        [HttpPut("{id:guid}/roles")]
        public async Task<ActionResult<Result>> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result.NotFound("User not found"));

                var roles = request.Roles ?? new List<string>();
                var roleError = await ValidateRolesAsync(roles);
                if (roleError != null)
                    return BadRequest(Result.ValidationFailure(roleError, new List<string> { roleError }));

                var errors = await SyncRolesAsync(user, roles);
                if (errors != null)
                    return BadRequest(Result.Failure("Could not update roles", errors));

                return Ok(Result.Success("Roles updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", id);
                return StatusCode(500, Result.InternalError("Error updating user roles"));
            }
        }

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<Result>> SetUserStatus(string id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result.NotFound("User not found"));

                user.IsActive = request.IsActive;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(Result.Failure("Could not update user status", Describe(result)));

                return Ok(Result.Success(request.IsActive ? "User activated" : "User deactivated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for user {UserId}", id);
                return StatusCode(500, Result.InternalError("Error updating user status"));
            }
        }

        /// <summary>
        /// POST api/users/{id}/change-password.
        /// With <c>CurrentPassword</c> it behaves as a self-service change; without it, it is an
        /// administrative reset (no old password needed). Both paths run the configured password
        /// validators and rotate the security stamp, which invalidates existing sessions.
        /// </summary>
        [HttpPost("{id:guid}/change-password")]
        public async Task<ActionResult<Result>> ChangePassword(string id, [FromBody] ChangePasswordDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest(Result.ValidationFailure("New password is required",
                        new List<string> { "New password is required" }));

                if (!string.IsNullOrEmpty(request.ConfirmNewPassword) && request.ConfirmNewPassword != request.NewPassword)
                    return BadRequest(Result.ValidationFailure("Passwords do not match",
                        new List<string> { "Passwords do not match" }));

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound(Result.NotFound("User not found"));

                IdentityResult result;
                if (!string.IsNullOrEmpty(request.CurrentPassword))
                {
                    result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                }
                else
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                }

                if (!result.Succeeded)
                    return BadRequest(Result.Failure("Could not change password", Describe(result)));

                _logger.LogInformation("Password changed for user {UserId}", id);
                return Ok(Result.Success("Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, Result.InternalError("Error changing password"));
            }
        }

        // ---- helpers -------------------------------------------------------------------------

        private static IQueryable<ApplicationUser> ApplySearch(IQueryable<ApplicationUser> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var term = search.Trim().ToLower();
            return query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        private async Task<List<UserDto>> ToDtosAsync(IEnumerable<ApplicationUser> users)
        {
            var dtos = new List<UserDto>();
            foreach (var user in users)
                dtos.Add(await ToDtoAsync(user));
            return dtos;
        }

        private async Task<UserDto> ToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedOn = user.CreatedOn,
                LastLogin = user.LastLogin,
                Roles = roles.ToList()
            };
        }

        /// <summary>Returns an error message if any requested role does not exist; otherwise null.</summary>
        private async Task<string?> ValidateRolesAsync(IEnumerable<string>? roles)
        {
            foreach (var role in (roles ?? Enumerable.Empty<string>()).Distinct())
            {
                if (string.IsNullOrWhiteSpace(role) || !await _roleManager.RoleExistsAsync(role))
                    return $"Role '{role}' does not exist";
            }
            return null;
        }

        /// <summary>Makes the user's roles exactly <paramref name="desired"/>. Returns error descriptions or null on success.</summary>
        private async Task<List<string>?> SyncRolesAsync(ApplicationUser user, IEnumerable<string> desired)
        {
            var target = desired.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var current = await _userManager.GetRolesAsync(user);

            var toRemove = current.Where(r => !target.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
            var toAdd = target.Where(r => !current.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

            if (toRemove.Count > 0)
            {
                var removed = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removed.Succeeded) return Describe(removed);
            }
            if (toAdd.Count > 0)
            {
                var added = await _userManager.AddToRolesAsync(user, toAdd);
                if (!added.Succeeded) return Describe(added);
            }
            return null;
        }

        private static List<string> Describe(IdentityResult result) =>
            result.Errors.Select(e => e.Description).ToList();
    }

    public class UpdateUserRolesRequest
    {
        public List<string>? Roles { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
