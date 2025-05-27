using IAMS.Application.DTOs.Identity;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IIdentityService identityService,
            ILogger<AuthController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var result = await _identityService.LoginAsync(loginDto.Email, loginDto.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for {Email}", loginDto.Email);
                return Unauthorized(new { Message = result.ErrorMessage });
            }

            return Ok(new AuthResponseDto
            {
                UserId = result.User.UserId,
                Email = result.User.Email,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
                JwtToken = result.User.JwtToken,
                RefreshToken = result.User.RefreshToken,
                Roles = result.User.Roles,
                Permissions = result.User.Permissions
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _identityService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = result.ErrorMessage });
            }

            return Ok(new AuthResponseDto
            {
                UserId = result.User.UserId,
                Email = result.User.Email,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
                JwtToken = result.User.JwtToken,
                RefreshToken = result.User.RefreshToken,
                Roles = result.User.Roles,
                Permissions = result.User.Permissions
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            var result = await _identityService.RegisterUserAsync(registerDto);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors });
            }

            return Ok(new { UserId = result.UserId });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            await _identityService.RevokeTokenAsync(userId);
            return Ok();
        }
    }
}
