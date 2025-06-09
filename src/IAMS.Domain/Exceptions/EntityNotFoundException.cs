namespace IAMS.Domain.Exceptions
{
    public class EntityNotFoundException : DomainException
    {
        public string EntityType { get; }
        public int EntityId { get; }

        public EntityNotFoundException(string entityType, int entityId, int tenantId = 0)
            : base("ENTITY_NOT_FOUND", $"{entityType} with ID {entityId} was not found", tenantId)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityType, string identifier, int tenantId = 0)
            : base("ENTITY_NOT_FOUND", $"{entityType} with identifier '{identifier}' was not found", tenantId)
        {
            EntityType = entityType;
            EntityId = 0;
        }
    }
}