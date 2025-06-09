namespace IAMS.Domain.Exceptions
{
    public class TenantNotFoundException : DomainException
    {
        public string TenantIdentifier { get; }

        public TenantNotFoundException(string tenantIdentifier)
            : base("TENANT_NOT_FOUND", $"Tenant with identifier '{tenantIdentifier}' was not found")
        {
            TenantIdentifier = tenantIdentifier;
        }

        public TenantNotFoundException(int tenantId)
            : base("TENANT_NOT_FOUND", $"Tenant with ID {tenantId} was not found")
        {
            TenantIdentifier = tenantId.ToString();
        }
    }
}