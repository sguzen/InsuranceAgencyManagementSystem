using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace IAMS.Admin.Services
{
    public class AdminAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminAuthenticationStateProvider> _logger;

        public AdminAuthenticationStateProvider(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AdminAuthenticationStateProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Admin user authenticated: {Name}",
                    httpContext.User.Identity.Name);
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            _logger.LogInformation("Admin user is not authenticated");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}
