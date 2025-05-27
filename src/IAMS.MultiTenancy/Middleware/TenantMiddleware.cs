// IAMS.MultiTenancy/Middleware/TenantMiddleware.cs (Enhanced)
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.MultiTenancy.Services;

namespace IAMS.MultiTenancy.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantService _tenantService;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(
            RequestDelegate next,
            ITenantService tenantService,
            ITenantContextAccessor tenantContextAccessor,
            ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _tenantService = tenantService;
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Extract tenant identifier from the request
                var tenantIdentifier = ExtractTenantIdentifier(context);

                if (string.IsNullOrEmpty(tenantIdentifier))
                {
                    _logger.LogWarning("No tenant identifier found in request: {Request}", context.Request.Path);
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Tenant identifier is required");
                    return;
                }

                // Get tenant information
                var tenant = await _tenantService.GetTenantAsync(tenantIdentifier);

                if (tenant == null)
                {
                    _logger.LogWarning("Unknown tenant attempted to access the system: {TenantIdentifier}", tenantIdentifier);
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tenant not found");
                    return;
                }

                // Check if tenant is active
                if (!tenant.IsActive)
                {
                    _logger.LogWarning("Inactive tenant attempted to access the system: {TenantId}", tenant.Id);
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Tenant account is inactive");
                    return;
                }

                // Check subscription status
                if (!tenant.IsSubscriptionActive())
                {
                    _logger.LogWarning("Tenant with expired subscription attempted access: {TenantId}", tenant.Id);
                    context.Response.StatusCode = 402;
                    await context.Response.WriteAsync("Subscription has expired");
                    return;
                }

                // Set tenant context
                var tenantContext = new TenantContext(tenant);
                _tenantContextAccessor.TenantContext = tenantContext;

                // Add tenant information to HTTP context items for easy access
                context.Items["CurrentTenant"] = tenant;
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantIdentifier"] = tenant.Identifier;

                _logger.LogDebug("Tenant context set for request: {TenantId} - {TenantIdentifier}", tenant.Id, tenant.Identifier);

                // Continue to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant middleware");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private string ExtractTenantIdentifier(HttpContext context)
        {
            // Strategy 1: Extract from subdomain
            var host = context.Request.Host.Value.ToLowerInvariant();

            // Check if it's a subdomain (tenant.yourdomain.com)
            if (host.Contains('.'))
            {
                var parts = host.Split('.');
                if (parts.Length >= 3) // subdomain.domain.com
                {
                    return parts[0];
                }
            }

            // Strategy 2: Extract from custom header
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            // Strategy 3: Extract from query parameter (for development/testing)
            if (context.Request.Query.TryGetValue("tenant", out var queryValue))
            {
                return queryValue.FirstOrDefault();
            }

            // Strategy 4: Extract from path (api/{tenant}/...)
            var path = context.Request.Path.Value;
            if (path.StartsWith("/api/") && path.Length > 5)
            {
                var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length >= 2 && pathParts[0] == "api")
                {
                    return pathParts[1];
                }
            }

            // Default for development
            return "default";
        }
    }
}