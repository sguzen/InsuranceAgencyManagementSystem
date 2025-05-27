using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IAMS.Identity.Models;
using IAMS.Application.DTOs.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IPermissionService permissionService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                return new AuthResult { Succeeded = false, ErrorMessage = "Invalid credentials" };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                return new AuthResult { Succeeded = false, ErrorMessage = "Invalid credentials" };
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate tokens
            var jwtToken = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays"));
            await _userManager.UpdateAsync(user);

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            return new AuthResult
            {
                Succeeded = true,
                User = new AuthUserDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JwtToken = jwtToken,
                    RefreshToken = refreshToken,
                    Roles = roles.ToList(),
                    Permissions = permissions
                }
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow || !user.IsActive)
            {
                return new AuthResult { Succeeded = false, ErrorMessage = "Invalid refresh token" };
            }

            // Generate new tokens
            var jwtToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays"));
            await _userManager.UpdateAsync(user);

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            return new AuthResult
            {
                Succeeded = true,
                User = new AuthUserDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    JwtToken = jwtToken,
                    RefreshToken = newRefreshToken,
                    Roles = roles.ToList(),
                    Permissions = permissions
                }
            };
        }

        public async Task<RegisterResult> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new RegisterResult
                {
                    Succeeded = false,
                    Errors = new List<string> { "User with this email already exists" }
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                EmailConfirmed = true, // For now, auto-confirm emails
                TenantId = 1 // TODO: Get from current tenant context
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new RegisterResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign role if specified
            if (!string.IsNullOrEmpty(registerDto.RoleName))
            {
                await _userManager.AddToRoleAsync(user, registerDto.RoleName);
            }

            return new RegisterResult
            {
                Succeeded = true,
                UserId = user.Id
            };
        }

        public async Task RevokeTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userManager.UpdateAsync(user);
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new("tenant_id", user.TenantId.ToString())
            };

            // Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add permission claims
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryMinutes")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}