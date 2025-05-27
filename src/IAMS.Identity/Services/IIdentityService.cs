using IAMS.Application.DTOs.Identity;
using IAMS.Identity.Models;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace IAMS.Identity.Services
{
    public interface IIdentityService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<RegisterResult> RegisterUserAsync(RegisterUserDto registerDto);
        Task RevokeTokenAsync(string userId);
    }

    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public AuthUserDto? User { get; set; }
    }

    public class AuthUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class RegisterResult
    {
        public bool Succeeded { get; set; }
        public string? UserId { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}