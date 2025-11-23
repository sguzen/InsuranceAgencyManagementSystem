using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace IAMS.Infrastructure.Logging.Enrichers
{
    public class UserEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
                return;

            var user = httpContext.User;

            // Add UserId
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            }

            // Add Username/Email
            var username = user.FindFirst(ClaimTypes.Name)?.Value
                           ?? user.FindFirst("name")?.Value
                           ?? user.FindFirst(ClaimTypes.Email)?.Value
                           ?? user.FindFirst("email")?.Value;
            if (!string.IsNullOrEmpty(username))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Username", username));
            }

            // Add User Roles
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            if (roles.Length > 0)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRoles", roles));
            }
        }
    }
}
