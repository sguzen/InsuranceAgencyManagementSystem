namespace IAMS.Domain.Exceptions
{
    public class UnauthorizedTenantAccessException : DomainException
    {
        public int RequestedTenantId { get; }
        public int UserTenantId { get; }

        public UnauthorizedTenantAccessException(int requestedTenantId, int userTenantId)
            : base("UNAUTHORIZED_TENANT_ACCESS",
                   $"User from tenant {userTenantId} is not authorized to access tenant {requestedTenantId}")
        {
            RequestedTenantId = requestedTenantId;
            UserTenantId = userTenantId;
        }
    }
}