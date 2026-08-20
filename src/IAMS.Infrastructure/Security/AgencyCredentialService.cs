using IAMS.MultiTenancy.Data;
using IAMS.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySqlConnector;

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

                if (IsMySqlServer(entity.DbServer))
                {
                    // Parse host:port for MySQL
                    var (host, port) = ParseHostPort(entity.DbServer, 3306);
                    return $"Server={host};Port={port};Database={entity.DbName};" +
                           $"User Id={entity.DbUsername};Password={password};" +
                           "SslMode=Preferred;";
                }

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

                if (IsMySqlConnectionString(connectionString))
                {
                    await using var connection = new MySqlConnection(connectionString);
                    await connection.OpenAsync();
                    await connection.CloseAsync();
                }
                else
                {
                    using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                    await connection.OpenAsync();
                    await connection.CloseAsync();
                }

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

        public async Task<AgencyCredentialDetails?> GetCredentialDetailsAsync(int agencyId, int insuranceCompanyId)
        {
            var entity = await _context.AgencyInsuranceCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AgencyId == agencyId
                    && x.InsuranceCompanyId == insuranceCompanyId
                    && !x.IsDeleted
                    && x.IsActive);

            if (entity == null || string.IsNullOrEmpty(entity.DbServer) || string.IsNullOrEmpty(entity.DbName))
                return null;

            var isMySql = IsMySqlServer(entity.DbServer);
            var (host, port) = ParseHostPort(entity.DbServer, isMySql ? 3306 : 1433);
            var password = !string.IsNullOrEmpty(entity.DbPassword)
                ? _encryption.Decrypt(entity.DbPassword)
                : "";

            // The agency code is assigned by each insurer, so it lives on the link.
            // Fall back to the tenant-level ExternalId only for links created before
            // per-insurer codes existed.
            var agencyCode = entity.AgencyCode?.Trim();
            if (string.IsNullOrEmpty(agencyCode))
            {
                var tenant = await _context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == agencyId);
                agencyCode = tenant?.ExternalId?.Trim();

                _logger.LogWarning(
                    "No insurer-specific AgencyCode configured for Agency {AgencyId} / InsuranceCompany {InsuranceCompanyId}; " +
                    "falling back to tenant ExternalId '{ExternalId}'. Set the agency code on the agency-insurance company link.",
                    agencyId, insuranceCompanyId, agencyCode ?? "(none)");
            }

            return new AgencyCredentialDetails
            {
                Host = host,
                Port = port,
                DatabaseName = entity.DbName,
                Username = entity.DbUsername ?? "",
                Password = password,
                IsMySql = isMySql,
                AgencyCode = string.IsNullOrEmpty(agencyCode) ? null : agencyCode
            };
        }

        /// <summary>
        /// Detects if a server address is likely MySQL based on port number.
        /// Common MySQL ports: 3306, 23306, 33306. SQL Server default: 1433.
        /// </summary>
        private static bool IsMySqlServer(string server)
        {
            if (server.Contains(':'))
            {
                var portStr = server.Split(':').Last();
                if (int.TryParse(portStr, out var port))
                {
                    return port == 3306 || port % 10000 == 3306;
                }
            }
            return false;
        }

        /// <summary>
        /// Detects if a connection string is MySQL format (has SslMode or Port keywords).
        /// </summary>
        private static bool IsMySqlConnectionString(string connectionString)
        {
            return connectionString.Contains("SslMode=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase);
        }

        private static (string Host, int Port) ParseHostPort(string server, int defaultPort)
        {
            var parts = server.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var port))
            {
                return (parts[0], port);
            }
            return (server, defaultPort);
        }
    }
}
