namespace IAMS.Domain.Entities
{
    public class Policy : BaseEntity
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
        public virtual PolicyType PolicyType { get; set; } = null!;
        public virtual ICollection<PolicyPayment> PolicyPayments { get; set; } = new List<PolicyPayment>();
        public virtual ICollection<PolicyClaim> PolicyClaims { get; set; } = new List<PolicyClaim>();
    }

    public enum PolicyStatus
    {
        Draft = 0,
        Active = 1,
        Expired = 2,
        Cancelled = 3,
        Suspended = 4
    }
}