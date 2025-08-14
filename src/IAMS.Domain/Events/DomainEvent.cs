using IAMS.Domain.Interfaces;

namespace IAMS.Domain.Events
{
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
}