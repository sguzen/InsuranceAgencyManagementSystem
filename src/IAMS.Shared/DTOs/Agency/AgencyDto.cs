using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Agency
{
    public class AgencyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? ExternalId { get; set; }
        public AgencyStatus Status { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? TrialExpiryDate { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int MaxUsers { get; set; }
        public int MaxPolicies { get; set; }
        public string? ModuleSettings { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
    }

    public class CreateAgencyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? ExternalId { get; set; }
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Basic;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int MaxUsers { get; set; } = 5;
        public int MaxPolicies { get; set; } = 1000;
        public DateTime? SubscriptionExpiryDate { get; set; }
    }

    public class UpdateAgencyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string? ExternalId { get; set; }
        public AgencyStatus Status { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int MaxUsers { get; set; }
        public int MaxPolicies { get; set; }
        public DateTime? SubscriptionExpiryDate { get; set; }
    }
}
