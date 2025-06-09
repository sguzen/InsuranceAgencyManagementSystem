using IAMS.Domain.Interfaces;
using System.Security.Principal;

namespace IAMS.Domain.Entities
{
    public abstract class BaseEntity : ITenantEntity, IEntity, IAuditable, ISoftDeletable, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public int Id { get; set; }
        public int TenantId { get; set; }

        // Auditable properties
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }

        // Soft delete properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }

        // Domain events
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // Helper methods
        public virtual void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            DeletedOn = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public virtual void Restore()
        {
            IsDeleted = false;
            DeletedOn = null;
            DeletedBy = null;
        }

        public virtual void UpdateAuditInfo(string modifiedBy)
        {
            ModifiedOn = DateTime.UtcNow;
            ModifiedBy = modifiedBy;
        }

        // Virtual validation method that can be overridden by derived classes
        protected virtual void Validate()
        {
            // Base validation logic can be added here
        }
    }
}