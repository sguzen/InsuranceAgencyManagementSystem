namespace IAMS.Domain.Entities
{
    public class InsuranceCompany : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? ApiEndpoint { get; set; }
        public string? ApiKey { get; set; }
        public string? IntegrationSettings { get; set; } // JSON settings

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; } = new List<CustomerInsuranceCompany>();
        public virtual ICollection<CommissionRate> CommissionRates { get; set; } = new List<CommissionRate>();
    }
}