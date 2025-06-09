using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyCancelledEvent : DomainEvent
    {
        public Policy Policy { get; }
        public string CancelledBy { get; }
        public string? CancellationReason { get; }
        public DateTime CancellationDate { get; }

        public PolicyCancelledEvent(Policy policy, string cancelledBy, string? cancellationReason = null)
            : base(policy.TenantId)
        {
            Policy = policy;
            CancelledBy = cancelledBy;
            CancellationReason = cancellationReason;
            CancellationDate = DateTime.UtcNow;
        }
    }
}