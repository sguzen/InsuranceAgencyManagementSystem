using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAMS.MultiTenancy.Entities
{
    [Table("TenantSettings")]
    public class TenantSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string SettingValue { get; set; } = string.Empty;

        [MaxLength(50)]
        public string SettingType { get; set; } = "string";

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual TenantEntity Tenant { get; set; }
    }
}