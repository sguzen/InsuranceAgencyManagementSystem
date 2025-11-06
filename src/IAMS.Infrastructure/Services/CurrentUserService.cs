using IAMS.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IAMS.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                                   ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? FirstName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value;

        public string? LastName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value;

      //  public  => int.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value ?? "1");

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public List<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                                        ?.Select(c => c.Value)?.ToList() ?? new List<string>();

        public List<string> Permissions => _httpContextAccessor.HttpContext?.User?.FindAll("permission")
                                              ?.Select(c => c.Value)?.ToList() ?? new List<string>();

        public ClaimsPrincipal? GetClaimsPrincipal() => _httpContextAccessor.HttpContext?.User;

        public bool HasPermission(string permission) => Permissions.Contains(permission);

        public bool HasRole(string role) => Roles.Contains(role);

        public bool HasAnyRole(params string[] roles) => roles.Any(role => Roles.Contains(role));

        public bool HasAnyPermission(params string[] permissions) => permissions.Any(permission => Permissions.Contains(permission));
    }
}