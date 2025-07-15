using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyPremiumUpdatedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public decimal OldPremium { get; }
        public decimal NewPremium { get; }
        public string UpdatedBy { get; }

        public PolicyPremiumUpdatedEvent(Policy policy, decimal oldPremium, decimal newPremium, string updatedBy)
            : base(policy.TenantId)
        {
            Policy = policy;
            OldPremium = oldPremium;
            NewPremium = newPremium;
            UpdatedBy = updatedBy;
        }
    }
}