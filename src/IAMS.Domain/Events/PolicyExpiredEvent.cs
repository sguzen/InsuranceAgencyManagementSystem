using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyExpiredEvent : DomainEvent
    {
        public Policy Policy { get; }
        public DateTime ExpiryDate { get; }

        public PolicyExpiredEvent(Policy policy) : base()
        {
            Policy = policy;
            ExpiryDate = policy.EndDate;
        }
    }
}