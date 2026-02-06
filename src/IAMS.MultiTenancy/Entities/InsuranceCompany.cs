using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAMS.MultiTenancy.Entities
{
    /// <summary>
    /// Master list of insurance companies in the system.
    /// Stored in the master tenant database.
    /// </summary>
    [Table("InsuranceCompanies")]
    public class InsuranceCompany
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(200)]
        public string? ApiEndpoint { get; set; }

        [MaxLength(1000)]
        public string? IntegrationSettings { get; set; } // JSON for API keys, config, etc.

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        // Audit fields
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation property
        public virtual ICollection<AgencyInsuranceCompany> AgencyInsuranceCompanies { get; set; } = new List<AgencyInsuranceCompany>();
    }
}
