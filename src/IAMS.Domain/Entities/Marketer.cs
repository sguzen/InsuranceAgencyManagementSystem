namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Represents a marketer (pazarlamacı) who brings customers to the agency
    /// </summary>
    public class Marketer : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Pazarlama Kodu
        public string Name { get; set; } = string.Empty; // Pazarlama Adı
        public string? ContactPerson { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public decimal? CommissionRate { get; set; } // Optional commission rate for the marketer
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();

        // Business methods
        public void Activate(string activatedBy)
        {
            IsActive = true;
            UpdateAuditInfo(activatedBy);
        }

        public void Deactivate(string deactivatedBy)
        {
            IsActive = false;
            UpdateAuditInfo(deactivatedBy);
        }
    }
}
