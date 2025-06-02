using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Interfaces
{
    public interface ITenantService
    {
        Task<Tenant> GetTenantAsync(string identifier);
        Task<Tenant> GetTenantByIdAsync(int tenantId);
        Task<List<Tenant>> GetAllActiveTenantsAsync();
        Task InvalidateTenantCacheAsync(string identifier);
        Task InvalidateTenantCacheAsync(int tenantId);
        Tenant GetCurrentTenant();
        int? GetCurrentTenantId();
        bool IsModuleEnabledForCurrentTenant(string moduleName);
        Task UpdateTenantModuleAsync(int tenantId, string moduleName, bool isEnabled);
        Task UpdateTenantSettingAsync(int tenantId, string settingKey, object value, string settingType = "string");
    }
}