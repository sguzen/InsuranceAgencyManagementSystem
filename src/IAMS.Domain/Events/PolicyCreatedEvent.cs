using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyCreatedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public string CreatedBy { get; }

        public PolicyCreatedEvent(Policy policy, string createdBy) : base(policy.TenantId)
        {
            Policy = policy;
            CreatedBy = createdBy;
        }
    }
}