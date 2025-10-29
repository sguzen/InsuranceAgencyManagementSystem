using IAMS.Domain.Entities;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Ülkeler - Parametric table for countries (nationality)
    /// Examples: 052-Türkiye, 601-KKTC
    /// </summary>
    public class Country : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // 052, 601, etc.
        public string NameTr { get; set; } = string.Empty; // Turkish name
        public string NameEn { get; set; } = string.Empty; // English name
        public string? PhoneCode { get; set; } // +90, +392, etc.
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual ICollection<Customer> CustomersWithNationality { get; set; } = new List<Customer>();
    }
}