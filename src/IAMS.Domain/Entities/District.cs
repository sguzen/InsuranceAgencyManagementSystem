using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Mahalleler - Parametric table for districts/neighborhoods
    /// </summary>
    public class District : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // District code
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public int CityId { get; set; } // Foreign key to City
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual City City { get; set; } = null!;
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<Subdistrict> Subdistricts { get; set; } = new List<Subdistrict>();
    }
}