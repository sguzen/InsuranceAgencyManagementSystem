using IAMS.Domain.Enums;
using System.Linq;

namespace IAMS.Domain.Entities
{
    public class InsuranceCompany : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
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
            return Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted);
        }

        public int GetActiveCustomerCount()
        {
            return CustomerInsuranceCompanies.Count(c => c.IsActive);
        }

        public static InsuranceCompany Create(string name, string? description, string? contactEmail, string? contactPhone, string? address, string? website, string createdBy)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Insurance company name is required", nameof(name));
            }

            var company = new InsuranceCompany
            {
                Name = name,
                Code = GenerateCompanyCode(name),
                Description = description,
                Email = contactEmail ?? string.Empty,
                Phone = contactPhone ?? string.Empty,
                Address = address ?? string.Empty,
                Website = website,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

            return company;
        }

        private static string GenerateCompanyCode(string name)
        {
            // Generate a code from the company name (first 3 chars + timestamp)
            var prefix = new string(name.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper();
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "INS"; // TODO ensure uniqueness of a total of 10 chars
            }
            //var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"{prefix}";
        }
    }
}