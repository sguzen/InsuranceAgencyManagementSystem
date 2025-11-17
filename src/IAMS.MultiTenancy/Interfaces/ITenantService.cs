using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Interfaces
{
    /// <summary>
    /// Service for managing tenant metadata stored in the MASTER database.
    /// This service handles tenant resolution, module management, and cache invalidation.
    /// Note: Tenant-specific settings are stored in each tenant's own database (see IAMS.Application.Interfaces.ITenantService).
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Gets tenant information by identifier from the master database.
        /// Results are cached for performance.
        /// </summary>
        Task<Tenant> GetTenantAsync(string identifier);

        /// <summary>
        /// Gets tenant information by ID from the master database.
        /// </summary>
        Task<Tenant> GetTenantByIdAsync();

        /// <summary>
        /// Gets all active tenants from the master database.
        /// </summary>
        Task<List<Tenant>> GetAllActiveTenantsAsync();

        /// <summary>
        /// Invalidates the cached tenant information by identifier.
        /// Call this after updating tenant metadata in the master database.
        /// </summary>
        Task InvalidateTenantCacheAsync(string identifier);

        /// <summary>
        /// Invalidates the cached tenant information by tenant ID.
        /// </summary>
        Task InvalidateTenantCacheAsync();

        /// <summary>
        /// Gets the current tenant from the request context.
        /// </summary>
        Tenant GetCurrentTenant();

        /// <summary>
        /// Gets the current tenant ID from the request context.
        /// </summary>
        int? GetCurrentTenantId();

        /// <summary>
        /// Checks if a module is enabled for the current tenant.
        /// Module information is stored in the master database's TenantModules table.
        /// </summary>
        bool IsModuleEnabledForCurrentTenant(string moduleName);

        /// <summary>
        /// Updates whether a module is enabled for a tenant.
        /// This updates the TenantModules table in the MASTER database.
        /// </summary>
        Task UpdateTenantModuleAsync(string moduleName, bool isEnabled);
    }
}