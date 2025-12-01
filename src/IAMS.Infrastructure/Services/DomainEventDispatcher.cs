using IAMS.Application.Interfaces;
using IAMS.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Dispatches domain events using MediatR
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(
            IMediator mediator,
            ILogger<DomainEventDispatcher> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogDebug("Successfully dispatched domain event: {EventType}", domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event: {EventType}", domainEvent.GetType().Name);
                throw;
            }
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            var eventsList = domainEvents.ToList();

            if (!eventsList.Any())
            {
                return;
            }

            _logger.LogDebug("Dispatching {Count} domain events", eventsList.Count);

            foreach (var domainEvent in eventsList)
            {
                await DispatchAsync(domainEvent, cancellationToken);
            }

            _logger.LogDebug("Successfully dispatched all {Count} domain events", eventsList.Count);
        }
    }
}
