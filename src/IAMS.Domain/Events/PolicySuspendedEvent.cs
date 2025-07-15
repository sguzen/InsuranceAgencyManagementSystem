using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicySuspendedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public string SuspendedBy { get; }
        public string? SuspensionReason { get; }
        public DateTime SuspensionDate { get; }

        public PolicySuspendedEvent(Policy policy, string suspendedBy, string? suspensionReason = null)
            : base(policy.TenantId)
        {
            Policy = policy;
            SuspendedBy = suspendedBy;
            SuspensionReason = suspensionReason;
            SuspensionDate = DateTime.UtcNow;
        }
    }
}