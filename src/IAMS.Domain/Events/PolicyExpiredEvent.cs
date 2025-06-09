using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyExpiredEvent : DomainEvent
    {
        public Policy Policy { get; }
        public DateTime ExpiryDate { get; }

        public PolicyExpiredEvent(Policy policy) : base(policy.TenantId)
        {
            Policy = policy;
            ExpiryDate = policy.EndDate;
        }
    }
}