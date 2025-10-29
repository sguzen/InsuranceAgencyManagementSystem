using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Şehirler - Parametric table for KKTC cities
    /// </summary>
    public class City : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // City code
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<District> Districts { get; set; } = new List<District>();
    }
}






