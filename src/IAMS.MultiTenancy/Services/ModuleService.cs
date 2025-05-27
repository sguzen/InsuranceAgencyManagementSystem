using IAMS.MultiTenancy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.MultiTenancy.Services
{
    public class ModuleService : IModuleService
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public ModuleService(ITenantContextAccessor tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        public bool IsModuleEnabled(string moduleName)
        {
            var tenant = _tenantContextAccessor.TenantContext?.Tenant;

            if (tenant == null)
            {
                return false;
            }

            return tenant.EnabledModules.TryGetValue(moduleName, out bool isEnabled) && isEnabled;
        }

        public IEnumerable<string> GetEnabledModules()
        {
            var tenant = _tenantContextAccessor.TenantContext?.Tenant;

            if (tenant == null)
            {
                return Enumerable.Empty<string>();
            }

            return tenant.EnabledModules
                .Where(m => m.Value)
                .Select(m => m.Key);
        }
    }
}
