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
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }

        /// <summary>
        /// Links to the master InsuranceCompany in TenantDb.
        /// Used to identify this company across the system.
        /// </summary>
        public int? MasterInsuranceCompanyId { get; set; }

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

        /// <summary>
        /// Indicates if this company is linked to the master database.
        /// Integration credentials are managed in the master database.
        /// </summary>
        public bool HasMasterLink => MasterInsuranceCompanyId.HasValue;

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
            // Generate a 10-character unique code: 3-char prefix + 7-char timestamp
            var prefix = new string(name.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper();
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "INS";
            }

            // Pad prefix to 3 chars if needed
            prefix = prefix.PadRight(3, 'X');

            // Add 7-digit timestamp suffix for uniqueness including milliseconds
            var timestamp = DateTime.UtcNow;
            var uniqueSuffix = ((timestamp.Day * 100000000L) +
                               (timestamp.Hour * 1000000L) +
                               (timestamp.Minute * 10000L) +
                               (timestamp.Second * 1000L) +
                               timestamp.Millisecond) % 10000000;

            return $"{prefix}{uniqueSuffix:D7}";
        }
    }
}