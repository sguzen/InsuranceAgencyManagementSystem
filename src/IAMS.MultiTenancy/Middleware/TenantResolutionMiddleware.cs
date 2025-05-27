using IAMS.MultiTenancy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IAMS.MultiTenancy.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public TenantResolutionMiddleware(
            RequestDelegate next,
            ILogger<TenantResolutionMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            var tenantIdentifier = ResolveTenantIdentifier(context);

            if (!string.IsNullOrEmpty(tenantIdentifier))
            {
                // In a real implementation, you would fetch tenant details from database
                tenantContext.TenantIdentifier = tenantIdentifier;
                tenantContext.TenantId = GetTenantId(tenantIdentifier);
                tenantContext.TenantName = GetTenantName(tenantIdentifier);

                _logger.LogInformation("Resolved tenant: {TenantIdentifier} (ID: {TenantId})",
                    tenantIdentifier, tenantContext.TenantId);
            }
            else
            {
                // Default tenant
                var defaultTenant = _configuration["MultiTenancy:DefaultTenant"] ?? "default";
                tenantContext.TenantIdentifier = defaultTenant;
                tenantContext.TenantId = 1;
                tenantContext.TenantName = "Default Agency";

                _logger.LogInformation("Using default tenant: {DefaultTenant}", defaultTenant);
            }

            await _next(context);
        }

        private string? ResolveTenantIdentifier(HttpContext context)
        {
            var strategy = _configuration["MultiTenancy:Strategy"];

            return strategy?.ToLower() switch
            {
                "header" => ResolveTenantFromHeader(context),
                "subdomain" => ResolveTenantFromSubdomain(context),
                "database" => ResolveTenantFromDatabase(context),
                _ => ResolveTenantFromHeader(context) // Default to header
            };
        }

        private string? ResolveTenantFromHeader(HttpContext context)
        {
            var headerName = _configuration["MultiTenancy:HeaderName"] ?? "X-Tenant-ID";
            return context.Request.Headers[headerName].FirstOrDefault();
        }

        private string? ResolveTenantFromSubdomain(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');

            // Expecting format: tenant.domain.com
            if (parts.Length >= 3)
            {
                return parts[0];
            }

            return null;
        }

        private string? ResolveTenantFromDatabase(HttpContext context)
        {
            // This would involve a database lookup based on some context
            // For now, return null to use default tenant
            return null;
        }

        private int GetTenantId(string tenantIdentifier)
        {
            // In a real implementation, this would be a database lookup
            // For now, use a simple hash-based ID generation
            return Math.Abs(tenantIdentifier.GetHashCode() % 1000) + 1;
        }

        private string GetTenantName(string tenantIdentifier)
        {
            // In a real implementation, this would be a database lookup
            return $"{tenantIdentifier} Insurance Agency";
        }
    }

    // Extension method to register the middleware
    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
}