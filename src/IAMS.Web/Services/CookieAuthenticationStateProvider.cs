using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace IAMS.Web.Services
{
    public class CookieAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CookieAuthenticationStateProvider> _logger;

        public CookieAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor)
            : base(loggerFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = loggerFactory.CreateLogger<CookieAuthenticationStateProvider>();
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        protected override Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            // Return whether the user is still authenticated
            return Task.FromResult(authenticationState.User.Identity?.IsAuthenticated ?? false);
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogDebug("User is authenticated: {Name}", httpContext.User.Identity.Name);
                return Task.FromResult(new AuthenticationState(httpContext.User));
            }

            _logger.LogDebug("User is not authenticated");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}