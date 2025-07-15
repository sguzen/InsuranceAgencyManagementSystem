using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyRenewedEvent : DomainEvent
    {
        public Policy OriginalPolicy { get; }
        public Policy RenewalPolicy { get; }
        public string RenewedBy { get; }

        public PolicyRenewedEvent(Policy originalPolicy, Policy renewalPolicy, string renewedBy)
            : base(originalPolicy.TenantId)
        {
            OriginalPolicy = originalPolicy;
            RenewalPolicy = renewalPolicy;
            RenewedBy = renewedBy;
        }
    }
}