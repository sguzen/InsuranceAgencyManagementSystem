using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAMS.MultiTenancy.Entities
{
    /// <summary>
    /// Represents the association between an agency (tenant) and an insurance company,
    /// including the connection string and credentials to the insurance company's database.
    /// Stored in the master tenant database.
    /// </summary>
    [Table("AgencyInsuranceCompanies")]
    public class AgencyInsuranceCompany
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AgencyId { get; set; }

        [Required]
        public int InsuranceCompanyId { get; set; }

        [MaxLength(100)]
        public string InsuranceCompanyName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string InsuranceCompanyCode { get; set; } = string.Empty;

        /// <summary>
        /// Code assigned to this agency by this insurance company in the insurer's own
        /// system (the "ackod" value used to filter policies on import).
        /// Each insurer assigns its own code, so it lives on the link, not on the agency.
        /// </summary>
        [MaxLength(IAMS.Shared.Validation.AgencyCodeRules.MaxLength)]
        public string? AgencyCode { get; set; }

        // Database connection details for this insurance company
        [MaxLength(200)]
        public string? DbServer { get; set; }

        [MaxLength(200)]
        public string? DbName { get; set; }

        [MaxLength(200)]
        public string? DbUsername { get; set; }

        [MaxLength(500)]
        public string? DbPassword { get; set; }

        [MaxLength(1000)]
        public string? ConnectionString { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Audit fields
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(AgencyId))]
        public virtual TenantEntity Agency { get; set; } = null!;

        [ForeignKey(nameof(InsuranceCompanyId))]
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
    }
}
