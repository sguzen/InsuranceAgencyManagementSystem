// IAMS.MultiTenancy/Services/CurrentTenantService.cs
using IAMS.Shared.Interfaces;
using IAMS.MultiTenancy.Interfaces;

namespace IAMS.MultiTenancy.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public CurrentTenantService(ITenantContextAccessor tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        public int? TenantId => _tenantContextAccessor.CurrentTenantId;

        public string? TenantName => _tenantContextAccessor.CurrentTenant?.Name;

        public bool HasTenant => _tenantContextAccessor.HasTenantContext;

        public bool IsModuleEnabled(string moduleName)
        {
            return _tenantContextAccessor.IsModuleEnabled(moduleName);
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            return _tenantContextAccessor.GetTenantSetting(key, defaultValue);
        }
    }
}