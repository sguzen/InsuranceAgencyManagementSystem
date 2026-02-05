using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Represents the association between a tenant (agency) and an insurance company,
    /// including the connection string and credentials to the insurance company's database.
    /// </summary>
    public class TenantInsuranceCompany : BaseEntity
    {
        public int TenantId { get; set; }
        public int InsuranceCompanyId { get; set; }

        // Database connection details for this insurance company
        public string? DbServer { get; set; }
        public string? DbName { get; set; }
        public string? DbUsername { get; set; }
        public string? DbPassword { get; set; }
        public string? ConnectionString { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
    }
}
