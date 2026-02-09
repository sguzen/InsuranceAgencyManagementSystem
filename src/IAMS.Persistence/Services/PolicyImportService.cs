using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.Shared.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IAMS.Persistence.Services
{
    /// <summary>
    /// Background service that processes policy import jobs.
    /// Reads jobs from master database (TenantDb) and imports policies to tenant databases.
    /// </summary>
    public class PolicyImportBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PolicyImportBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public PolicyImportBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<PolicyImportBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PolicyImportBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingJobsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing import jobs");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPendingJobsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var masterDb = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            // Get pending jobs from master database
            var pendingJobs = await masterDb.ImportJobs
                .Where(j => j.Status == ImportJobStatus.Pending)
                .OrderBy(j => j.CreatedOn)
                .Take(5) // Process up to 5 jobs at a time
                .ToListAsync(stoppingToken);

            foreach (var job in pendingJobs)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await ProcessJobAsync(scope.ServiceProvider, job, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(IServiceProvider serviceProvider, ImportJob job, CancellationToken stoppingToken)
        {
            var masterDb = serviceProvider.GetRequiredService<TenantDbContext>();
            var importService = serviceProvider.GetRequiredService<IPolicyImportService>();

            _logger.LogInformation("Processing import job {JobId} for Agency {AgencyId}, InsuranceCompany {InsuranceCompanyId}",
                job.Id, job.AgencyId, job.InsuranceCompanyId);

            job.Start();
            await masterDb.SaveChangesAsync(stoppingToken);

            try
            {
                var result = await importService.ImportPoliciesAsync(
                    job.AgencyId,
                    job.InsuranceCompanyId,
                    job.FilterStartDate,
                    job.FilterEndDate,
                    stoppingToken);

                job.Complete(result.Imported, result.Failed, result.Skipped);
                job.ImportLog = result.Log;

                _logger.LogInformation("Import job {JobId} completed: {Imported} imported, {Failed} failed, {Skipped} skipped",
                    job.Id, result.Imported, result.Failed, result.Skipped);
            }
            catch (Exception ex)
            {
                job.Fail(ex.Message);
                _logger.LogError(ex, "Import job {JobId} failed", job.Id);
            }

            await masterDb.SaveChangesAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Service for importing policies from external insurance company databases.
    /// </summary>
    public interface IPolicyImportService
    {
        Task<ImportResult> ImportPoliciesAsync(
            int agencyId,
            int insuranceCompanyId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken = default);
    }

    public class PolicyImportService : IPolicyImportService
    {
        private readonly IAgencyCredentialService _credentialService;
        private readonly ILogger<PolicyImportService> _logger;

        public PolicyImportService(
            IAgencyCredentialService credentialService,
            ILogger<PolicyImportService> logger)
        {
            _credentialService = credentialService;
            _logger = logger;
        }

        public async Task<ImportResult> ImportPoliciesAsync(
            int agencyId,
            int insuranceCompanyId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            var result = new ImportResult();
            var logEntries = new List<string>();

            try
            {
                logEntries.Add($"Starting import at {DateTime.UtcNow:u}");
                logEntries.Add($"Agency: {agencyId}, InsuranceCompany: {insuranceCompanyId}");

                // Get connection string securely from master database
                var connectionString = await _credentialService.GetConnectionStringAsync(agencyId, insuranceCompanyId);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("No connection credentials configured for this insurance company");
                }

                logEntries.Add("Connection credentials retrieved");

                // Connect to external database
                using var externalConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await externalConnection.OpenAsync(cancellationToken);
                logEntries.Add("Connected to external database");

                // TODO: Implement actual policy import logic here
                // This would:
                // 1. Query the external database for policies
                // 2. Get tenant connection string for the agency
                // 3. Transform data to match our schema
                // 4. Insert/update policies in tenant database
                // 5. Track imported, failed, skipped counts

                // Placeholder - actual implementation depends on the external DB schema
                logEntries.Add("Policy import logic would execute here");
                logEntries.Add("External database schema needs to be configured per insurance company");

                await externalConnection.CloseAsync();
                logEntries.Add($"Import completed at {DateTime.UtcNow:u}");

                result.Log = string.Join("\n", logEntries);
            }
            catch (Exception ex)
            {
                logEntries.Add($"ERROR: {ex.Message}");
                result.Log = string.Join("\n", logEntries);
                throw;
            }

            return result;
        }
    }

    public class ImportResult
    {
        public int Imported { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public string? Log { get; set; }
    }
}
