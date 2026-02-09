using IAMS.Shared.Interfaces;
using IAMS.MultiTenancy.Data;
using IAMS.Persistence.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IAMS.Persistence.Services
{
    /// <summary>
    /// Service for managing tenant databases.
    /// Reads connection strings from the master database (Tenants table) instead of appsettings.
    /// Falls back to appsettings TenantConnections only if not found in database.
    /// </summary>
    public class TenantDatabaseService : ITenantDatabaseService
    {
        private readonly TenantDbContext _masterDb;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantDatabaseService> _logger;

        public TenantDatabaseService(
            TenantDbContext masterDb,
            IConfiguration configuration,
            ILogger<TenantDatabaseService> logger)
        {
            _masterDb = masterDb;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureTenantDatabaseAsync(string tenantIdentifier)
        {
            try
            {
                var connectionString = await GetTenantConnectionStringAsync(tenantIdentifier);

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new ApplicationDbContext(options, null);

                var canConnect = await context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    _logger.LogInformation("Creating database for tenant {TenantIdentifier}", tenantIdentifier);
                    await MigrateWithLockHandlingAsync(context, tenantIdentifier);
                }
                else
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        _logger.LogInformation("Applying migrations for tenant {TenantIdentifier}", tenantIdentifier);
                        await MigrateWithLockHandlingAsync(context, tenantIdentifier);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring database for tenant {TenantIdentifier}", tenantIdentifier);
                throw;
            }
        }

        public async Task CreateTenantDatabaseAsync(string tenantIdentifier)
        {
            var connectionString = await GetTenantConnectionStringAsync(tenantIdentifier);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new ApplicationDbContext(options, null);
            // await context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Created database for tenant {TenantIdentifier}", tenantIdentifier);
        }

        /// <summary>
        /// Runs MigrateAsync with handling for the EF Core 9 migration lock release error (SQL error 1223).
        /// The __EFMigrationsLock distributed lock can fail to release even when the migration succeeds.
        /// </summary>
        private async Task MigrateWithLockHandlingAsync(ApplicationDbContext context, string tenantIdentifier)
        {
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (SqlException ex) when (ex.Number == 1223)
            {
                // SQL error 1223: "Cannot release the application lock ... because it is not currently held."
                // This is a known EF Core 9 issue where the migration lock release fails.
                // Verify that migrations actually completed before swallowing the error.
                var pending = await context.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    _logger.LogError(ex, "Migration lock error for tenant {TenantIdentifier} and migrations are still pending", tenantIdentifier);
                    throw;
                }

                _logger.LogWarning(ex, "EF Core migration lock release failed for tenant {TenantIdentifier}, but all migrations were applied successfully", tenantIdentifier);
            }
        }

        /// <summary>
        /// Gets the connection string for a tenant.
        /// Priority: 1) Database (Tenants table) 2) appsettings TenantConnections 3) Default with replaced DB name
        /// </summary>
        private async Task<string> GetTenantConnectionStringAsync(string tenantIdentifier)
        {
            // 1. Try to get from database (recommended for production)
            var tenant = await _masterDb.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == tenantIdentifier && !t.IsDeleted && t.IsActive);

            if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
            {
                _logger.LogDebug("Using database connection string for tenant {TenantIdentifier}", tenantIdentifier);
                return tenant.ConnectionString;
            }

            // 2. Fallback to appsettings TenantConnections (for backwards compatibility)
            var tenantConnections = _configuration.GetSection("TenantConnections");
            var connectionString = tenantConnections[tenantIdentifier];

            if (!string.IsNullOrEmpty(connectionString))
            {
                _logger.LogDebug("Using appsettings TenantConnections for tenant {TenantIdentifier}", tenantIdentifier);
                return connectionString;
            }

            // 3. Fallback: use DefaultConnection with tenant identifier as database name
            var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(defaultConnection))
            {
                _logger.LogDebug("Using modified DefaultConnection for tenant {TenantIdentifier}", tenantIdentifier);
                return defaultConnection.Replace("ApplicationDb", tenantIdentifier);
            }

            throw new InvalidOperationException($"No connection string found for tenant '{tenantIdentifier}'");
        }
    }
}
