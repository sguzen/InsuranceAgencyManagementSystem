namespace IAMS.Domain.Interfaces
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
        int TenantId { get; }
    }
}