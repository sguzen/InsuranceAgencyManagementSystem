using IAMS.MultiTenancy.Data;
using IAMS.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.Security
{
    public class AgencyCredentialService : IAgencyCredentialService
    {
        private readonly TenantDbContext _context;
        private readonly ICredentialEncryptionService _encryption;
        private readonly ILogger<AgencyCredentialService> _logger;

        public AgencyCredentialService(
            TenantDbContext context,
            ICredentialEncryptionService encryption,
            ILogger<AgencyCredentialService> logger)
        {
            _context = context;
            _encryption = encryption;
            _logger = logger;
        }

        public async Task SaveCredentialsAsync(int agencyInsuranceCompanyId, string? dbServer, string? dbName,
            string? dbUsername, string? dbPassword, string? connectionString)
        {
            var entity = await _context.AgencyInsuranceCompanies
                .FirstOrDefaultAsync(x => x.Id == agencyInsuranceCompanyId);

            if (entity == null)
                throw new InvalidOperationException($"AgencyInsuranceCompany {agencyInsuranceCompanyId} not found");

            entity.DbServer = dbServer;
            entity.DbName = dbName;
            entity.DbUsername = dbUsername;

            // Encrypt sensitive fields
            if (!string.IsNullOrEmpty(dbPassword))
            {
                entity.DbPassword = _encryption.Encrypt(dbPassword);
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                entity.ConnectionString = _encryption.Encrypt(connectionString);
            }

            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Credentials updated for AgencyInsuranceCompany {Id}", agencyInsuranceCompanyId);
        }

        public async Task<string?> GetConnectionStringAsync(int agencyId, int insuranceCompanyId)
        {
            var entity = await _context.AgencyInsuranceCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AgencyId == agencyId
                    && x.InsuranceCompanyId == insuranceCompanyId
                    && !x.IsDeleted
                    && x.IsActive);

            if (entity == null)
            {
                _logger.LogWarning("No active link found for Agency {AgencyId} and InsuranceCompany {InsuranceCompanyId}",
                    agencyId, insuranceCompanyId);
                return null;
            }

            // If full connection string is provided, use it
            if (!string.IsNullOrEmpty(entity.ConnectionString))
            {
                return _encryption.Decrypt(entity.ConnectionString);
            }

            // Build connection string from parts
            if (!string.IsNullOrEmpty(entity.DbServer) && !string.IsNullOrEmpty(entity.DbName))
            {
                var password = !string.IsNullOrEmpty(entity.DbPassword)
                    ? _encryption.Decrypt(entity.DbPassword)
                    : "";

                return $"Server={entity.DbServer};Database={entity.DbName};" +
                       $"User Id={entity.DbUsername};Password={password};" +
                       "TrustServerCertificate=True;";
            }

            return null;
        }

        public async Task<(bool Success, string Message)> TestConnectionAsync(int agencyId, int insuranceCompanyId)
        {
            try
            {
                var connectionString = await GetConnectionStringAsync(agencyId, insuranceCompanyId);

                if (string.IsNullOrEmpty(connectionString))
                {
                    return (false, "No connection information configured");
                }

                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                await connection.CloseAsync();

                _logger.LogInformation("Connection test successful for Agency {AgencyId}, InsuranceCompany {InsuranceCompanyId}",
                    agencyId, insuranceCompanyId);

                return (true, "Connection successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for Agency {AgencyId}, InsuranceCompany {InsuranceCompanyId}",
                    agencyId, insuranceCompanyId);

                return (false, $"Connection failed: {ex.Message}");
            }
        }
    }
}
