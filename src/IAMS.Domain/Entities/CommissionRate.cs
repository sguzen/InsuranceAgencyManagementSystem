namespace IAMS.Domain.Entities
{
    public class CommissionRate : BaseEntity
    {
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public decimal Rate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
        public virtual PolicyType PolicyType { get; set; } = null!;
    }
}