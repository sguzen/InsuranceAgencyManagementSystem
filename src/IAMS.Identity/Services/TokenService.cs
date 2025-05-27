using IAMS.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public TokenService(
            IConfiguration configuration,
            ITenantContextAccessor tenantContextAccessor)
        {
            _configuration = configuration;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("first_name", user.FirstName ?? string.Empty),
            new Claim("last_name", user.LastName ?? string.Empty)
        };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Get tenant-specific settings
            var tenant = _tenantContextAccessor.TenantContext.Tenant;
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
        }
    }

    // IAMS.Identity/Models/RefreshToken.cs
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }
}
