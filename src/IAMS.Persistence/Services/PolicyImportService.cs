using IAMS.Application.Features.Policies.Commands.SyncPoliciesFromMySql;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IAMS.Persistence.Services
{
    /// <summary>
    /// Background service that processes policy import jobs.
    /// Reads jobs from master database (TenantDb), resolves tenant context,
    /// then delegates to IPolicyImportService running within the correct tenant scope.
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
                .Take(5)
                .ToListAsync(stoppingToken);

            foreach (var job in pendingJobs)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await ProcessJobAsync(masterDb, job, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(TenantDbContext masterDb, ImportJob job, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing import job {JobId} for Agency {AgencyId}, InsuranceCompany {InsuranceCompanyId}",
                job.Id, job.AgencyId, job.InsuranceCompanyId);

            // 1. Load the tenant from master DB to get connection string
            var tenantEntity = await masterDb.Tenants
                .AsNoTracking()
                .Include(t => t.TenantModules)
                .FirstOrDefaultAsync(t => t.Id == job.AgencyId && !t.IsDeleted, stoppingToken);

            if (tenantEntity == null)
            {
                job.Fail($"Tenant (agency) not found with ID: {job.AgencyId}");
                await masterDb.SaveChangesAsync(stoppingToken);
                return;
            }

            var tenant = MapToTenant(tenantEntity);

            job.Start();
            await masterDb.SaveChangesAsync(stoppingToken);

            try
            {
                // 2. Set tenant context so ApplicationDbContext resolves to the correct DB
                //    Create an inner scope so all scoped services (DbContext, UnitOfWork, MediatR)
                //    are resolved with the tenant connection string
                using var innerScope = _serviceProvider.CreateScope();
                var tenantContextAccessor = innerScope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();

                var result = await tenantContextAccessor.ExecuteWithTenantAsync(tenant, async () =>
                {
                    // All services resolved here will use the tenant's connection string
                    var importService = innerScope.ServiceProvider.GetRequiredService<IPolicyImportService>();

                    return await importService.ImportPoliciesAsync(
                        job.AgencyId,
                        job.InsuranceCompanyId,
                        job.FilterStartDate,
                        job.FilterEndDate,
                        job.RequestedBy,
                        stoppingToken);
                });

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

        private static Tenant MapToTenant(TenantEntity entity)
        {
            return new Tenant
            {
                Id = entity.Id,
                Name = entity.Name,
                Identifier = entity.Identifier,
                ConnectionString = entity.ConnectionString,
                IsActive = entity.IsActive,
                SubscriptionPlan = entity.SubscriptionPlan,
                MaxUsers = entity.MaxUsers,
                MaxStorageBytes = entity.MaxStorageBytes,
                ContactEmail = entity.ContactEmail,
                TimeZone = entity.TimeZone,
                Currency = entity.Currency,
                Language = entity.Language,
                EnabledModules = entity.TenantModules?
                    .ToDictionary(m => m.ModuleName, m => m.IsEnabled)
                    ?? new Dictionary<string, bool>(),
                Settings = new Dictionary<string, object>()
            };
        }
    }

    /// <summary>
    /// Service for importing policies from external insurance company databases.
    /// Runs within a tenant-scoped context (set by the background service).
    /// </summary>
    public interface IPolicyImportService
    {
        Task<ImportResult> ImportPoliciesAsync(
            int agencyId,
            int masterInsuranceCompanyId,
            DateTime? startDate,
            DateTime? endDate,
            string requestedBy,
            CancellationToken cancellationToken = default);
    }

    public class PolicyImportService : IPolicyImportService
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PolicyImportService> _logger;

        public PolicyImportService(
            IMediator mediator,
            IUnitOfWork unitOfWork,
            ILogger<PolicyImportService> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ImportResult> ImportPoliciesAsync(
            int agencyId,
            int masterInsuranceCompanyId,
            DateTime? startDate,
            DateTime? endDate,
            string requestedBy,
            CancellationToken cancellationToken = default)
        {
            var result = new ImportResult();
            var logEntries = new List<string>();

            try
            {
                logEntries.Add($"Starting import at {DateTime.UtcNow:u}");
                logEntries.Add($"Agency: {agencyId}, MasterInsuranceCompanyId: {masterInsuranceCompanyId}");

                // Find the tenant's InsuranceCompany that corresponds to the master DB ID
                var tenantInsuranceCompany = await _unitOfWork.InsuranceCompanies
                    .GetByMasterIdAsync(masterInsuranceCompanyId);

                if (tenantInsuranceCompany == null)
                {
                    throw new InvalidOperationException(
                        $"No insurance company found in tenant database with MasterInsuranceCompanyId={masterInsuranceCompanyId}. " +
                        "Ensure the insurance company was synced to the tenant database.");
                }

                logEntries.Add($"Found tenant insurance company: {tenantInsuranceCompany.Name} (ID: {tenantInsuranceCompany.Id})");

                // Default date range: last year to today
                var effectiveStartDate = startDate ?? DateTime.Today.AddYears(-1);
                var effectiveEndDate = endDate ?? DateTime.Today;

                logEntries.Add($"Date range: {effectiveStartDate:yyyy-MM-dd} to {effectiveEndDate:yyyy-MM-dd}");

                // Send the MediatR command which handles:
                //   1. Loading ImportConfiguration (MySQL connection details)
                //   2. Fetching policies from external MySQL database
                //   3. Creating/updating Customers, Vehicles, Policies in tenant DB
                var command = new SyncPoliciesFromMySqlCommand(
                    effectiveStartDate,
                    effectiveEndDate,
                    tenantInsuranceCompany.Id,
                    requestedBy ?? "system-import");

                var commandResult = await _mediator.Send(command, cancellationToken);

                if (commandResult.IsSuccess && commandResult.Data != null)
                {
                    var data = commandResult.Data;
                    result.Imported = data.SuccessCount;
                    result.Failed = data.FailureCount;
                    result.Skipped = data.TotalRows - data.SuccessCount - data.FailureCount;

                    logEntries.Add($"Import completed: {data.SuccessCount} imported, {data.FailureCount} failed");

                    if (data.Errors.Any())
                    {
                        logEntries.Add("Errors:");
                        foreach (var error in data.Errors.Take(50))
                        {
                            logEntries.Add($"  Row {error.RowNumber} ({error.PolicyNumber}): {error.ErrorMessage}");
                        }
                        if (data.Errors.Count > 50)
                            logEntries.Add($"  ... and {data.Errors.Count - 50} more errors");
                    }
                }
                else
                {
                    var errorMsg = commandResult.ErrorMessage ?? "Unknown error during import";
                    throw new InvalidOperationException(errorMsg);
                }

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
