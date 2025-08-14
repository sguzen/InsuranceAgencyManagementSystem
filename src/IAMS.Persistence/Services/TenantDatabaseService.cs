using IAMS.Application.Interfaces;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IAMS.Persistence.Services
{
    public class TenantDatabaseService : ITenantDatabaseService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantDatabaseService> _logger;

        public TenantDatabaseService(
            IConfiguration configuration,
            ILogger<TenantDatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureTenantDatabaseAsync(string tenantIdentifier)
        {
            try
            {
                var connectionString = GetTenantConnectionString(tenantIdentifier);

                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new ApplicationDbContext(options, null);

                var canConnect = await context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    _logger.LogInformation("Creating database for tenant {TenantIdentifier}", tenantIdentifier);
                    await context.Database.EnsureCreatedAsync();
                }
                else
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        _logger.LogInformation("Applying migrations for tenant {TenantIdentifier}", tenantIdentifier);
                        await context.Database.MigrateAsync();
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
            var connectionString = GetTenantConnectionString(tenantIdentifier);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new ApplicationDbContext(options, null);
            await context.Database.EnsureCreatedAsync();

            _logger.LogInformation("Created database for tenant {TenantIdentifier}", tenantIdentifier);
        }

        private string GetTenantConnectionString(string tenantIdentifier)
        {
            var tenantConnections = _configuration.GetSection("TenantConnections");
            var connectionString = tenantConnections[tenantIdentifier];

            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(defaultConnection))
            {
                return defaultConnection.Replace("IAMS_Default", $"IAMS_{tenantIdentifier}");
            }

            throw new InvalidOperationException($"No connection string found for tenant '{tenantIdentifier}'");
        }
    }
}
