using IAMS.Domain.Interfaces;

namespace IAMS.Application.Interfaces
{
    /// <summary>
    /// Dispatcher for publishing domain events
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a single domain event
        /// </summary>
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispatches multiple domain events
        /// </summary>
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
