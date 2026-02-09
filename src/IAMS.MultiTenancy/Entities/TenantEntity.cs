using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IAMS.Domain.Enums;

namespace IAMS.MultiTenancy.Entities
{
    [Table("Tenants")]
    public class TenantEntity : ITenantEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Identifier { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ConnectionString { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdated { get; set; }

        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic";

        public DateTime? SubscriptionExpiry { get; set; }

        public int MaxUsers { get; set; } = 10;

        public long MaxStorageBytes { get; set; } = 1024 * 1024 * 1024; // 1GB

        [MaxLength(200)]
        public string ContactEmail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TimeZone { get; set; } = "UTC";

        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        [MaxLength(10)]
        public string Language { get; set; } = "en";

        [MaxLength(50)]
        public string? ExternalId { get; set; } // Agency number for display (e.g., "A001")

        // Agency-specific fields
        public AgencyStatus Status { get; set; } = AgencyStatus.Active;
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Basic;
        public DateTime? TrialExpiryDate { get; set; }
        public int MaxPolicies { get; set; } = 1000;
        public string? ModuleSettings { get; set; } // JSON for module on/off switches
        public string? Settings { get; set; } // JSON settings

        // Navigation properties
        public virtual ICollection<TenantModule> TenantModules { get; set; } = new List<TenantModule>();
        public virtual ICollection<AgencyInsuranceCompany> AgencyInsuranceCompanies { get; set; } = new List<AgencyInsuranceCompany>();
    }
}