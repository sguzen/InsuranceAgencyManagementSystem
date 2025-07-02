// IAMS.MultiTenancy/Middleware/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        // Only inject services that are safe to inject at startup (Singleton or Transient)
        public TenantMiddleware(
            RequestDelegate next,
            ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Get scoped services from the request's service provider
                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                var tenantContextAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();

                // Get tenant identifier from request
                var tenantIdentifier = GetTenantIdentifier(context);

                if (string.IsNullOrEmpty(tenantIdentifier))
                {
                    tenantIdentifier = "default"; // Use default tenant
                }

                // Get tenant from your service (which uses your TenantContext DbContext)
                var tenant = await tenantService.GetTenantAsync(tenantIdentifier);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant not found: {TenantIdentifier}", tenantIdentifier);
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tenant not found");
                    return;
                }

                if (!tenant.IsActive)
                {
                    _logger.LogWarning("Inactive tenant: {TenantId}", tenant.Id);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Tenant is inactive");
                    return;
                }

                // Set the current tenant in context accessor
                // This creates a TenantContext (the wrapper) with the Tenant (the model)
                tenantContextAccessor.TenantContext = new TenantContext(tenant);

                // Also add to HTTP context for easy access
                context.Items["CurrentTenant"] = tenant;
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantIdentifier"] = tenant.Identifier;

                _logger.LogDebug("Tenant set: {TenantId} - {TenantName}", tenant.Id, tenant.Name);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant middleware");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private static string GetTenantIdentifier(HttpContext context)
        {
            // Try header first
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
            {
                var tenantId = headerValue.FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantId))
                {
                    return tenantId;
                }
            }

            // Try subdomain
            var host = context.Request.Host.Value;
            if (host.Contains('.'))
            {
                var parts = host.Split('.');
                if (parts.Length >= 3 && parts[0] != "www")
                {
                    return parts[0];
                }
            }

            // Try query parameter
            if (context.Request.Query.TryGetValue("tenant", out var queryValue))
            {
                var tenantId = queryValue.FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantId))
                {
                    return tenantId;
                }
            }

            return "default";
        }
    }
}