using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Bucaklar - Parametric table for subdistricts
    /// </summary>
    public class Subdistrict : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Subdistrict code
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public int DistrictId { get; set; } // Foreign key to District
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual District District { get; set; } = null!;
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<Village> Villages { get; set; } = new List<Village>();
    }
}