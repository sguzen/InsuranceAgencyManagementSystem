// Middleware/TenantMiddleware.cs - Revised and Enhanced
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.MultiTenancy.Extensions;
using IAMS.MultiTenancy.Data;

namespace IAMS.MultiTenancy.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantService _tenantService;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ILogger<TenantMiddleware> _logger;
        private readonly MultiTenancyOptions _options;

        public TenantMiddleware(
            RequestDelegate next,
            ITenantService tenantService,
            ITenantContextAccessor tenantContextAccessor,
            ILogger<TenantMiddleware> logger,
            IOptions<MultiTenancyOptions>? options = null)
        {
            _next = next;
            _tenantService = tenantService;
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
            _options = options?.Value ?? new MultiTenancyOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip tenant resolution for health checks and static files
                if (ShouldSkipTenantResolution(context))
                {
                    await _next(context);
                    return;
                }

                // Extract tenant identifier from the request
                var tenantIdentifier = ResolveTenantIdentifier(context);

                if (string.IsNullOrEmpty(tenantIdentifier))
                {
                    // Use default tenant if configured
                    if (!string.IsNullOrEmpty(_options.DefaultTenant))
                    {
                        tenantIdentifier = _options.DefaultTenant;
                        _logger.LogDebug("Using default tenant: {TenantIdentifier}", tenantIdentifier);
                    }
                    else
                    {
                        _logger.LogWarning("No tenant identifier found in request: {Request}", context.Request.Path);
                        await HandleTenantNotFound(context, "Tenant identifier is required");
                        return;
                    }
                }

                // Get tenant information
                var tenant = await _tenantService.GetTenantAsync(tenantIdentifier);

                if (tenant == null)
                {
                    _logger.LogWarning("Unknown tenant attempted to access the system: {TenantIdentifier}", tenantIdentifier);
                    await HandleTenantNotFound(context, "Tenant not found");
                    return;
                }

                // Check if tenant is active
                if (!tenant.IsActive)
                {
                    _logger.LogWarning("Inactive tenant attempted to access the system: {TenantId}", tenant.Id);
                    await HandleTenantInactive(context, "Tenant account is inactive");
                    return;
                }

                // Check subscription status
                if (!tenant.IsSubscriptionActive())
                {
                    _logger.LogWarning("Tenant with expired subscription attempted access: {TenantId}", tenant.Id);
                    await HandleSubscriptionExpired(context, "Subscription has expired");
                    return;
                }

                // Set tenant context
                var tenantContext = new TenantContext(tenant);
                _tenantContextAccessor.TenantContext = tenantContext;

                // Add tenant information to HTTP context items for easy access
                context.Items["CurrentTenant"] = tenant;
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantIdentifier"] = tenant.Identifier;
                context.Items["TenantContext"] = tenantContext;

                _logger.LogDebug("Tenant context set for request: {TenantId} - {TenantIdentifier}", tenant.Id, tenant.Identifier);

                // Continue to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant middleware");
                await HandleError(context, "Internal server error occurred while resolving tenant");
            }
        }

        private string? ResolveTenantIdentifier(HttpContext context)
        {
            return _options.ResolutionStrategy switch
            {
                TenantResolutionStrategy.Header => ResolveTenantFromHeader(context),
                TenantResolutionStrategy.Subdomain => ResolveTenantFromSubdomain(context),
                TenantResolutionStrategy.Path => ResolveTenantFromPath(context),
                TenantResolutionStrategy.QueryParameter => ResolveTenantFromQuery(context),
                _ => ResolveTenantFromHeader(context) // Default fallback
            };
        }

        private string? ResolveTenantFromHeader(HttpContext context)
        {
            var headerName = _options.HeaderName;
            return context.Request.Headers.TryGetValue(headerName, out var headerValue)
                ? headerValue.FirstOrDefault()
                : null;
        }

        private string? ResolveTenantFromSubdomain(HttpContext context)
        {
            var host = context.Request.Host.Value.ToLowerInvariant();

            // Check if it's a subdomain (tenant.yourdomain.com)
            if (host.Contains('.'))
            {
                var parts = host.Split('.');
                if (parts.Length >= 3) // subdomain.domain.com
                {
                    var subdomain = parts[0];
                    // Skip common subdomains
                    if (!IsCommonSubdomain(subdomain))
                    {
                        return subdomain;
                    }
                }
            }

            return null;
        }

        private string? ResolveTenantFromPath(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path != null && path.StartsWith("/tenant/") && path.Length > 8)
            {
                var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length >= 2 && pathParts[0] == "tenant")
                {
                    return pathParts[1];
                }
            }

            return null;
        }

        private string? ResolveTenantFromQuery(HttpContext context)
        {
            return context.Request.Query.TryGetValue("tenant", out var queryValue)
                ? queryValue.FirstOrDefault()
                : null;
        }

        private static bool ShouldSkipTenantResolution(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            return path != null && (
                path.StartsWith("/health") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/api/health") ||
                path.StartsWith("/favicon.ico") ||
                path.Contains("/css/") ||
                path.Contains("/js/") ||
                path.Contains("/images/") ||
                path.Contains(".css") ||
                path.Contains(".js") ||
                path.Contains(".png") ||
                path.Contains(".jpg") ||
                path.Contains(".jpeg") ||
                path.Contains(".gif") ||
                path.Contains(".ico")
            );
        }

        private static bool IsCommonSubdomain(string subdomain)
        {
            var commonSubdomains = new[] { "www", "api", "admin", "mail", "ftp", "blog", "dev", "test", "staging" };
            return commonSubdomains.Contains(subdomain.ToLowerInvariant());
        }

        private async Task HandleTenantNotFound(HttpContext context, string message)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{message}\", \"code\": \"TENANT_NOT_FOUND\"}}");
        }

        private async Task HandleTenantInactive(HttpContext context, string message)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{message}\", \"code\": \"TENANT_INACTIVE\"}}");
        }

        private async Task HandleSubscriptionExpired(HttpContext context, string message)
        {
            context.Response.StatusCode = 402;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{message}\", \"code\": \"SUBSCRIPTION_EXPIRED\"}}");
        }

        private async Task HandleError(HttpContext context, string message)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{message}\", \"code\": \"INTERNAL_ERROR\"}}");
        }
    }
}