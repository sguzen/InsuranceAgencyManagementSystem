// IAMS.MultiTenancy/Middleware/TenantMiddleware.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.Shared.Constants;
using IAMS.Shared.Interfaces;

namespace IAMS.MultiTenancy.Middleware
{
    /// <summary>
    /// Resolves the tenant for the current request from trusted sources only:
    /// 1. X-Tenant-ID header — accepted only from internal service callers authenticated
    ///    with the shared API key (or when MultiTenancy:AllowClientTenantResolution is
    ///    explicitly enabled, e.g. local development / Swagger).
    /// 2. The tenant_id claim of an authenticated principal (signed into the JWT at login).
    /// 3. The request host's subdomain (each tenant has its own web address).
    /// 4. The configured MultiTenancy:DefaultTenant, if set (single-tenant deployments).
    ///
    /// If the authenticated principal carries a tenant claim that does not match the
    /// resolved tenant, the request is rejected with 403 — a user of tenant A can never
    /// operate on tenant B's database, whatever headers they send.
    ///
    /// NOTE: in pipelines where authentication runs after this middleware (e.g. the Web
    /// app's cookie authentication), context.User is unauthenticated here and only the
    /// host/default sources apply.
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;
        private readonly bool _allowClientTenantResolution;
        private readonly string? _defaultTenant;

        // Only inject services that are safe to inject at startup (Singleton or Transient)
        public TenantMiddleware(
            RequestDelegate next,
            ILogger<TenantMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _allowClientTenantResolution = configuration.GetValue<bool>("MultiTenancy:AllowClientTenantResolution");
            _defaultTenant = configuration["MultiTenancy:DefaultTenant"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var tenantService = context.RequestServices.GetRequiredService<Interfaces.ITenantService>();
                var tenantContextAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
                var tenantDbService = context.RequestServices.GetRequiredService<ITenantDatabaseService>();

                Tenant? tenant = null;
                foreach (var candidate in GetTenantIdentifierCandidates(context))
                {
                    tenant = await tenantService.GetTenantAsync(candidate);
                    if (tenant != null)
                    {
                        break;
                    }
                }

                if (tenant == null)
                {
                    _logger.LogWarning("No tenant could be resolved for host {Host}", context.Request.Host.Value);
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Tenant could not be resolved");
                    return;
                }

                // An authenticated user may only operate on the tenant their token was issued for.
                var claimTenant = context.User?.FindFirst(ApplicationConstants.ClaimTypes.TenantId)?.Value;
                if (context.User?.Identity?.IsAuthenticated == true &&
                    !string.IsNullOrEmpty(claimTenant) &&
                    !string.Equals(claimTenant, tenant.Identifier, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Tenant mismatch: principal belongs to {ClaimTenant} but request resolved to {ResolvedTenant}",
                        claimTenant, tenant.Identifier);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: tenant mismatch");
                    return;
                }

                if (!tenant.IsActive)
                {
                    _logger.LogWarning("Inactive tenant: {TenantId}", tenant.Id);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Tenant is inactive");
                    return;
                }

                // Ensure the tenant database exists and is migrated (cached per tenant per process)
                await tenantDbService.EnsureTenantDatabaseAsync(tenant.Identifier);

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
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private IEnumerable<string> GetTenantIdentifierCandidates(HttpContext context)
        {
            // X-Tenant-ID header — trusted only on the internal service hop (API key)
            // or when client resolution is explicitly enabled for development.
            if (context.Request.Headers.TryGetValue(ApplicationConstants.Headers.TenantId, out var headerValue))
            {
                var headerTenant = headerValue.FirstOrDefault();
                if (!string.IsNullOrEmpty(headerTenant) &&
                    (_allowClientTenantResolution || IsTrustedServiceCaller(context.User)))
                {
                    yield return headerTenant;
                }
                else if (!string.IsNullOrEmpty(headerTenant))
                {
                    _logger.LogWarning("Ignoring {Header} header from untrusted caller", ApplicationConstants.Headers.TenantId);
                }
            }

            // Tenant claim issued at login — cannot be forged without the signing key.
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var claimTenant = context.User.FindFirst(ApplicationConstants.ClaimTypes.TenantId)?.Value;
                if (!string.IsNullOrEmpty(claimTenant))
                {
                    yield return claimTenant;
                }
            }

            // Host subdomain — each tenant has its own web address.
            var host = context.Request.Host.Host;
            if (host.Contains('.'))
            {
                var parts = host.Split('.');
                if (parts.Length == 2 && parts[1].Contains("localhost"))
                {
                    yield return parts[0];
                }
                else if (parts.Length >= 3 && parts[0] != "www")
                {
                    yield return parts[0];
                }
            }

            // Explicitly configured fallback for single-tenant deployments.
            // Leave MultiTenancy:DefaultTenant unset to disable.
            if (!string.IsNullOrEmpty(_defaultTenant))
            {
                yield return _defaultTenant;
            }
        }

        private static bool IsTrustedServiceCaller(ClaimsPrincipal? user)
        {
            return user?.Identity?.IsAuthenticated == true &&
                   user.HasClaim(ApplicationConstants.ClaimTypes.ApiKeyValidated, "true");
        }
    }
}
