using IAMS.Application.DTOs.Identity;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            try
            {
                var result = await _identityService.LoginAsync(request.Email, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(result.User);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _identityService.RefreshTokenAsync(request.RefreshToken);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(result.User);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return StatusCode(500, new { message = "An error occurred during token refresh" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    await _identityService.RevokeTokenAsync(userId);
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            try
            {
                var result = await _identityService.RegisterUserAsync(request);

                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors });
                }

                return Ok(new { message = "User registered successfully", userId = result.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}