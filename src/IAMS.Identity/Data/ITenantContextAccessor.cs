// IAMS.Identity/Interfaces/ITenantContextAccessor.cs
using IAMS.MultiTenancy.Models;

namespace IAMS.Identity.Interfaces
{
    public interface ITenantContextAccessor
    {
        TenantContext TenantContext { get; }
    }

    public class TenantContext
    {
        public Tenant Tenant { get; set; }
        public bool IsResolved { get; set; }
    }

    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        // Add other tenant properties as needed
    }
}