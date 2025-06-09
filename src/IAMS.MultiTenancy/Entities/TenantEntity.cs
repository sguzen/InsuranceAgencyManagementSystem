using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

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
    }
}