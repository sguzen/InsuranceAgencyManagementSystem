using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace IAMS.Web.Services
{
    // This authentication state provider works with cookie authentication in Blazor Server
    public class CookieAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CookieAuthenticationStateProvider> _logger;

        public CookieAuthenticationStateProvider(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CookieAuthenticationStateProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is authenticated: {Name} with {ClaimCount} claims",
                    httpContext.User.Identity.Name,
                    httpContext.User.Claims.Count());
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            _logger.LogInformation("User is not authenticated");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}
