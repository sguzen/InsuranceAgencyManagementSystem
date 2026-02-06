using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    public class Agency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty; // Subdomain or unique identifier
        public string? ExternalId { get; set; } // Agency number for policy display (e.g., "A001")
        public string? ConnectionString { get; set; }
        public AgencyStatus Status { get; set; } = AgencyStatus.Active;
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Basic;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? TrialExpiryDate { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
        public string? Settings { get; set; } // JSON settings
        public string? ModuleSettings { get; set; } // JSON for module on/off switches
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int MaxUsers { get; set; } = 5;
        public int MaxPolicies { get; set; } = 1000;

        // Navigation properties
        public virtual ICollection<AgencyInsuranceCompany> AgencyInsuranceCompanies { get; set; } = new List<AgencyInsuranceCompany>();

        // Business methods
        public bool IsActive => Status == AgencyStatus.Active && !IsExpired;
        public bool IsExpired => SubscriptionExpiryDate.HasValue && SubscriptionExpiryDate < DateTime.Today;
        public bool IsInTrial => Status == AgencyStatus.Trial && TrialExpiryDate.HasValue && TrialExpiryDate >= DateTime.Today;

        public void Activate()
        {
            Status = AgencyStatus.Active;
        }

        public void Suspend()
        {
            Status = AgencyStatus.Suspended;
        }

        public void Expire()
        {
            Status = AgencyStatus.Expired;
        }

        public void UpgradeSubscription(SubscriptionType newType, DateTime expiryDate)
        {
            SubscriptionType = newType;
            SubscriptionExpiryDate = expiryDate;

            // Update limits based on subscription type
            switch (newType)
            {
                case SubscriptionType.Basic:
                    MaxUsers = 5;
                    MaxPolicies = 1000;
                    break;
                case SubscriptionType.Standard:
                    MaxUsers = 15;
                    MaxPolicies = 5000;
                    break;
                case SubscriptionType.Premium:
                    MaxUsers = 50;
                    MaxPolicies = 20000;
                    break;
                case SubscriptionType.Enterprise:
                    MaxUsers = 999;
                    MaxPolicies = 999999;
                    break;
            }
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(ModuleSettings))
                return false;

            // This would parse JSON in real implementation
            // For now, basic modules are always enabled
            return moduleName.ToLower() switch
            {
                "core" => true,
                "reporting" => SubscriptionType != SubscriptionType.Basic,
                "accounting" => SubscriptionType == SubscriptionType.Premium || SubscriptionType == SubscriptionType.Enterprise,
                "integration" => SubscriptionType == SubscriptionType.Enterprise,
                _ => false
            };
        }
    }
}
