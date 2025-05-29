using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Data
{
    public interface ITenantContextAccessor
    {
        TenantContext? TenantContext { get; }
        void SetTenant(TenantInfo tenant);
    }

    public class TenantContext
    {
        public TenantInfo Tenant { get; set; } = null!;
    }

    public class TenantInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class TenantContextAccessor : ITenantContextAccessor
    {
        private TenantContext? _tenantContext;

        public TenantContext? TenantContext => _tenantContext;

        public void SetTenant(TenantInfo tenant)
        {
            _tenantContext = new TenantContext { Tenant = tenant };
        }
    }
}
