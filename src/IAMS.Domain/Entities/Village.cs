using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Köyler - Parametric table for villages
    /// </summary>
    public class Village : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Village code
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public int SubdistrictId { get; set; } // Foreign key to Subdistrict
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual Subdistrict Subdistrict { get; set; } = null!;
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}