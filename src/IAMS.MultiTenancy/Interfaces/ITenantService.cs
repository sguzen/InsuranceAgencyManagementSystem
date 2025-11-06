using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Interfaces
{
    public interface ITenantService
    {
        Task<Tenant> GetTenantAsync(string identifier);
        Task<Tenant> GetTenantByIdAsync();
        Task<List<Tenant>> GetAllActiveTenantsAsync();
        Task InvalidateTenantCacheAsync(string identifier);
        Task InvalidateTenantCacheAsync();
        Tenant GetCurrentTenant();
        int? GetCurrentTenantId();
        bool IsModuleEnabledForCurrentTenant(string moduleName);
        Task UpdateTenantModuleAsync(string moduleName, bool isEnabled);
        Task UpdateTenantSettingAsync(string settingKey, object value, string settingType = "string");

    }
}