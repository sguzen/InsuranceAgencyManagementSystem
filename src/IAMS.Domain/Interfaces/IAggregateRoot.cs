using IAMS.Domain.Entities;
using System.Security.Principal;

namespace IAMS.Domain.Interfaces
{
    public interface IAggregateRoot : IEntity, ITenantEntity, IAuditable, ISoftDeletable, IHasDomainEvents
    {
    }
}