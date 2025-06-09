namespace IAMS.Domain.Exceptions
{
    public class DuplicateEntityException : DomainException
    {
        public string EntityType { get; }
        public string DuplicateField { get; }
        public string DuplicateValue { get; }

        public DuplicateEntityException(string entityType, string duplicateField, string duplicateValue, int tenantId = 0)
            : base("DUPLICATE_ENTITY", $"{entityType} with {duplicateField} '{duplicateValue}' already exists", tenantId)
        {
            EntityType = entityType;
            DuplicateField = duplicateField;
            DuplicateValue = duplicateValue;
        }
    }
}