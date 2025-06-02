// IAMS.Infrastructure/BackgroundServices/IntegrationSyncService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Data;
using IAMS.MultiTenancy.Interfaces;

namespace IAMS.Infrastructure.BackgroundServices
{
    public class IntegrationSyncService : TenantAwareBackgroundService
    {
        public IntegrationSyncService(
            IServiceProvider serviceProvider,
            ILogger<IntegrationSyncService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting integration sync job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationService>();
                    var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();

                    // Only run if integration module is enabled
                    if (await moduleService.IsModuleEnabledAsync("integration"))
                    {
                        try
                        {
                            // Test all provider connections
                            var providers = await integrationService.GetAvailableProvidersAsync();

                            foreach (var provider in providers.Where(p => p.IsEnabled))
                            {
                                var isConnected = await integrationService.TestConnectionAsync(provider.Name);
                                _logger.LogInformation("Provider {Provider} connection test: {Status}",
                                    provider.Name, isConnected ? "Success" : "Failed");
                            }

                            _logger.LogDebug("Completed integration sync for tenant {TenantId}", tenant.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error during integration sync for tenant {TenantId}", tenant.Id);
                        }
                    }
                });

                _logger.LogInformation("Completed integration sync job");

                // Run every 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    // IAMS.Infrastructure/BackgroundServices/ReportSchedulerService.cs
    public class ReportSchedulerService : TenantAwareBackgroundService
    {
        public ReportSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<ReportSchedulerService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting scheduled reports job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();
                    var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();

                    // Only run if reporting module is enabled
                    if (await moduleService.IsModuleEnabledAsync("reporting"))
                    {
                        try
                        {
                            var scheduledReports = await reportingService.GetScheduledReportsAsync();
                            var dueReports = scheduledReports.Where(r => r.IsActive &&
                                (r.NextRun == null || r.NextRun <= DateTime.UtcNow)).ToList();

                            foreach (var report in dueReports)
                            {
                                try
                                {
                                    await ProcessScheduledReportAsync(report, reportingService, scope);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to process scheduled report {ReportId}", report.Id);
                                }
                            }

                            _logger.LogDebug("Processed {Count} scheduled reports for tenant {TenantId}",
                                dueReports.Count, tenant.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing scheduled reports for tenant {TenantId}", tenant.Id);
                        }
                    }
                });

                _logger.LogInformation("Completed scheduled reports job");

                // Check every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessScheduledReportAsync(ScheduledReport report, IReportingService reportingService, IServiceScope scope)
        {
            try
            {
                _logger.LogInformation("Processing scheduled report {ReportName}", report.Name);

                // Generate the report
                var reportData = await reportingService.ExportReportAsync(report.ReportType, report.Parameters, "pdf");

                // Send via email
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var emailMessage = new EmailMessage
                {
                    Subject = $"Scheduled Report: {report.Name}",
                    Body = $"Please find the attached {report.Name} report generated on {DateTime.Now:dd/MM/yyyy HH:mm}.",
                    Attachments = new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            FileName = $"{report.Name}_{DateTime.Now:yyyyMMdd}.pdf",
                            Content = reportData,
                            ContentType = "application/pdf"
                        }
                    }
                };

                var recipients = report.EmailRecipients.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var recipient in recipients)
                {
                    emailMessage.ToEmail = recipient.Trim();
                    await emailService.SendAsync(emailMessage);
                }

                _logger.LogInformation("Successfully processed scheduled report {ReportName}", report.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process scheduled report {ReportName}", report.Name);
                throw;
            }
        }
    }

    // IAMS.Infrastructure/BackgroundServices/ClaimProcessingService.cs
    public class ClaimProcessingService : TenantAwareBackgroundService
    {
        public ClaimProcessingService(
            IServiceProvider serviceProvider,
            ILogger<ClaimProcessingService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting claim processing job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    try
                    {
                        // Process pending claims that need to be submitted to insurance companies
                        await ProcessPendingClaimsAsync(integrationService, emailService, tenant.Id);

                        // Check for claim status updates
                        await CheckClaimStatusUpdatesAsync(integrationService, emailService, tenant.Id);

                        _logger.LogDebug("Completed claim processing for tenant {TenantId}", tenant.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during claim processing for tenant {TenantId}", tenant.Id);
                    }
                });

                _logger.LogInformation("Completed claim processing job");

                // Run every 2 hours during business hours
                var now = DateTime.Now;
                var nextRun = now.Hour >= 8 && now.Hour <= 18 ?
                    TimeSpan.FromHours(2) :
                    TimeSpan.FromHours(12); // Less frequent outside business hours

                await Task.Delay(nextRun, stoppingToken);
            }
        }

        private async Task ProcessPendingClaimsAsync(IIntegrationService integrationService, IEmailService emailService, int tenantId)
        {
            // This would get pending claims from your database
            // For now, it's a placeholder
            var pendingClaimIds = new List<int>(); // Get from database

            foreach (var claimId in pendingClaimIds)
            {
                try
                {
                    var result = await integrationService.SubmitClaimAsync(claimId);

                    if (result.Success)
                    {
                        _logger.LogInformation("Successfully submitted claim {ClaimId}", claimId);

                        // Notify customer that claim was submitted
                        await emailService.SendClaimNotificationAsync(
                            "customer@example.com", // Get from claim data
                            "Customer Name", // Get from claim data
                            $"CLM-{claimId}",
                            "Sigorta şirketine gönderildi"
                        );
                    }
                    else
                    {
                        _logger.LogWarning("Failed to submit claim {ClaimId}: {Error}", claimId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error submitting claim {ClaimId}", claimId);
                }
            }
        }

        private async Task CheckClaimStatusUpdatesAsync(IIntegrationService integrationService, IEmailService emailService, int tenantId)
        {
            // This would check for claim status updates from insurance companies
            // Placeholder implementation
            _logger.LogDebug("Checking claim status updates for tenant {TenantId}", tenantId);
            await Task.CompletedTask;
        }
    }

    // IAMS.Infrastructure/BackgroundServices/DataCleanupService.cs
    public class DataCleanupService : TenantAwareBackgroundService
    {
        public DataCleanupService(
            IServiceProvider serviceProvider,
            ILogger<DataCleanupService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting data cleanup job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    try
                    {
                        // Clean up old integration logs (older than 6 months)
                        await CleanupIntegrationLogsAsync(scope, tenant.Id);

                        // Clean up temporary files
                        await CleanupTemporaryFilesAsync(scope, tenant.Id);

                        // Clean up old audit logs
                        await CleanupAuditLogsAsync(scope, tenant.Id);

                        _logger.LogDebug("Completed data cleanup for tenant {TenantId}", tenant.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during data cleanup for tenant {TenantId}", tenant.Id);
                    }
                });

                _logger.LogInformation("Completed data cleanup job");

                // Run daily at 2 AM
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var delay = nextRun - now;

                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        private async Task CleanupIntegrationLogsAsync(IServiceScope scope, int tenantId)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
            var cutoffDate = DateTime.UtcNow.AddMonths(-6);

            var oldLogs = dbContext.IntegrationLogs.Where(l => l.CreatedAt < cutoffDate);
            var count = await oldLogs.CountAsync();

            if (count > 0)
            {
                dbContext.IntegrationLogs.RemoveRange(oldLogs);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} old integration logs for tenant {TenantId}", count, tenantId);
            }
        }

        private async Task CleanupTemporaryFilesAsync(IServiceScope scope, int tenantId)
        {
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

            // Clean up files in temp folder older than 24 hours
            var tempFiles = await fileStorage.ListFilesAsync("temp");
            var cutoffDate = DateTime.UtcNow.AddHours(-24);

            var filesToDelete = tempFiles.Where(f => f.CreatedDate < cutoffDate).ToList();

            foreach (var file in filesToDelete)
            {
                try
                {
                    await fileStorage.DeleteAsync(file.Path);
                    _logger.LogDebug("Deleted temporary file {FilePath}", file.Path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary file {FilePath}", file.Path);
                }
            }

            if (filesToDelete.Any())
            {
                _logger.LogInformation("Cleaned up {Count} temporary files for tenant {TenantId}", filesToDelete.Count, tenantId);
            }
        }

        private async Task CleanupAuditLogsAsync(IServiceScope scope, int tenantId)
        {
            // Placeholder for audit log cleanup
            _logger.LogDebug("Audit log cleanup for tenant {TenantId}", tenantId);
            await Task.CompletedTask;
        }
    }

    // IAMS.Infrastructure/BackgroundServices/BackupService.cs
    public class BackupService : TenantAwareBackgroundService
    {
        public BackupService(
            IServiceProvider serviceProvider,
            ILogger<BackupService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting backup job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    try
                    {
                        // Create database backup
                        await CreateDatabaseBackupAsync(scope, tenant);

                        // Backup uploaded files
                        await BackupFilesAsync(scope, tenant);

                        _logger.LogInformation("Completed backup for tenant {TenantId}", tenant.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during backup for tenant {TenantId}", tenant.Id);
                    }
                });

                _logger.LogInformation("Completed backup job");

                // Run weekly on Sunday at 3 AM
                var now = DateTime.Now;
                var nextSunday = now.Date.AddDays(7 - (int)now.DayOfWeek).AddHours(3);
                var delay = nextSunday - now;

                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        private async Task CreateDatabaseBackupAsync(IServiceScope scope, object tenant)
        {
            // Placeholder for database backup logic
            _logger.LogInformation("Creating database backup for tenant");
            await Task.CompletedTask;
        }

        private async Task BackupFilesAsync(IServiceScope scope, object tenant)
        {
            // Placeholder for file backup logic
            _logger.LogInformation("Creating file backup for tenant");
            await Task.CompletedTask;
        }
    }
}