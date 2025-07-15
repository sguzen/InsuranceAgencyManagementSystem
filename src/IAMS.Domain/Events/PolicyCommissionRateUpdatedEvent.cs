using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyCommissionRateUpdatedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public decimal OldCommissionRate { get; }
        public decimal NewCommissionRate { get; }
        public string UpdatedBy { get; }

        public PolicyCommissionRateUpdatedEvent(Policy policy, decimal oldCommissionRate, decimal newCommissionRate, string updatedBy)
            : base(policy.TenantId)
        {
            Policy = policy;
            OldCommissionRate = oldCommissionRate;
            NewCommissionRate = newCommissionRate;
            UpdatedBy = updatedBy;
        }
    }
}