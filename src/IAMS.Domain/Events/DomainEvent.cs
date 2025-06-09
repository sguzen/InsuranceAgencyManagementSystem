using IAMS.Domain.Interfaces;

namespace IAMS.Domain.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; }
        public int TenantId { get; }

        protected DomainEvent(int tenantId)
        {
            OccurredOn = DateTime.UtcNow;
            TenantId = tenantId;
        }
    }
}