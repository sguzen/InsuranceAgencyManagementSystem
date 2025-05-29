using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAMS.MultiTenancy.Entities
{
    [Table("TenantModules")]
    public class TenantModule
    {
        [Key]
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        // Navigation property
        [ForeignKey(nameof(TenantId))]
        public virtual TenantEntity Tenant { get; set; } = null!;
    }
}