namespace IAMS.Domain.Entities
{
    public class CustomerInsuranceCompany : BaseEntity
    {
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string ExternalCustomerId { get; set; } = string.Empty; // Customer ID in insurance company's system
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;
    }
}