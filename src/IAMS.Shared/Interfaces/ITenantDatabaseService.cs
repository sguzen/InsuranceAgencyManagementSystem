namespace IAMS.Shared.Interfaces
{
    public interface ITenantDatabaseService
    {
        Task EnsureTenantDatabaseAsync(string tenantIdentifier);
        Task CreateTenantDatabaseAsync(string tenantIdentifier);
    }
}