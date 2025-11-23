using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace IAMS.Infrastructure.Logging.Enrichers
{
    public class TenantEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            // Add TenantId
            if (httpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));
            }

            // Add TenantIdentifier
            if (httpContext.Items.TryGetValue("TenantIdentifier", out var tenantIdentifier) && tenantIdentifier != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantIdentifier", tenantIdentifier));
            }

            // Add CurrentTenant Name if available
            if (httpContext.Items.TryGetValue("CurrentTenant", out var currentTenant) && currentTenant != null)
            {
                var tenant = currentTenant as IAMS.MultiTenancy.Models.Tenant;
                if (tenant != null)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantName", tenant.Name));
                }
            }
        }
    }
}
