using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyReactivatedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public string ReactivatedBy { get; }
        public DateTime ReactivationDate { get; }

        public PolicyReactivatedEvent(Policy policy, string reactivatedBy)
            : base(policy.TenantId)
        {
            Policy = policy;
            ReactivatedBy = reactivatedBy;
            ReactivationDate = DateTime.UtcNow;
        }
    }
}