using MediatR;

namespace IAMS.Domain.Interfaces
{
    /// <summary>
    /// Represents a domain event that can be published and handled by MediatR
    /// </summary>
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}