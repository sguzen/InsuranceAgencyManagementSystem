using IAMS.Domain.Enums;

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
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; } = new List<CustomerInsuranceCompany>();
        public virtual ICollection<CommissionRate> CommissionRates { get; set; } = new List<CommissionRate>();

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

        public bool HasIntegration => !string.IsNullOrWhiteSpace(ApiEndpoint);

        public CommissionRate? GetCommissionRate(int policyTypeId, DateTime effectiveDate)
        {
            return CommissionRates
                .Where(cr => cr.PolicyTypeId == policyTypeId
                            && cr.IsActive
                            && cr.EffectiveDate <= effectiveDate
                            && (cr.ExpiryDate == null || cr.ExpiryDate >= effectiveDate))
                .OrderByDescending(cr => cr.EffectiveDate)
                .FirstOrDefault();
        }

        public IEnumerable<Policy> GetActivePolicies()
        {
            return Policies.Where(p => p.IsActive && !p.IsDeleted);
        }

        public int GetActiveCustomerCount()
        {
            return CustomerInsuranceCompanies.Count(c => c.IsActive);
        }
    }
}