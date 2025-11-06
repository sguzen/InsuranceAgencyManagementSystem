// IAMS.Infrastructure/BackgroundServices/IntegrationSyncService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using IAMS.Application.Interfaces;
using IModuleService = IAMS.Application.Interfaces.IModuleService;

namespace IAMS.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for syncing integrations with external providers
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class IntegrationSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IntegrationSyncService> _logger;

        public IntegrationSyncService(
            IServiceProvider serviceProvider,
            ILogger<IntegrationSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting integration sync job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationService>();
                    var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();

                    // Only run if integration module is enabled
                    if (await moduleService.IsModuleEnabledAsync("integration"))
                    {
                        // Test all provider connections
                        var providers = await integrationService.GetAvailableProvidersAsync();

                        foreach (var provider in providers.Where(p => p.IsEnabled))
                        {
                            var isConnected = await integrationService.TestConnectionAsync(provider.Name);
                            _logger.LogInformation("Provider {Provider} connection test: {Status}",
                                provider.Name, isConnected ? "Success" : "Failed");
                        }

                        _logger.LogDebug("Completed integration sync");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during integration sync");
                }

                _logger.LogInformation("Completed integration sync job");

                // Run every 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background service for processing scheduled reports
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class ReportSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportSchedulerService> _logger;

        public ReportSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<ReportSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting scheduled reports job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var reportingService = scope.ServiceProvider.GetRequiredService<IReportingService>();
                    var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();

                    // Only run if reporting module is enabled
                    if (await moduleService.IsModuleEnabledAsync("reporting"))
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

                        _logger.LogDebug("Processed {Count} scheduled reports", dueReports.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scheduled reports");
                }

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

    /// <summary>
    /// Background service for processing claims
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class ClaimProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClaimProcessingService> _logger;

        public ClaimProcessingService(
            IServiceProvider serviceProvider,
            ILogger<ClaimProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting claim processing job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var integrationService = scope.ServiceProvider.GetRequiredService<IIntegrationService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    // Process pending claims that need to be submitted to insurance companies
                    await ProcessPendingClaimsAsync(integrationService, emailService);

                    // Check for claim status updates
                    await CheckClaimStatusUpdatesAsync(integrationService, emailService);

                    _logger.LogDebug("Completed claim processing");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during claim processing");
                }

                _logger.LogInformation("Completed claim processing job");

                // Run every 2 hours during business hours
                var now = DateTime.Now;
                var nextRun = now.Hour >= 8 && now.Hour <= 18 ?
                    TimeSpan.FromHours(2) :
                    TimeSpan.FromHours(12); // Less frequent outside business hours

                await Task.Delay(nextRun, stoppingToken);
            }
        }

        private async Task ProcessPendingClaimsAsync(IIntegrationService integrationService, IEmailService emailService)
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

        private async Task CheckClaimStatusUpdatesAsync(IIntegrationService integrationService, IEmailService emailService)
        {
            // This would check for claim status updates from insurance companies
            // Placeholder implementation
            _logger.LogDebug("Checking claim status updates");
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Background service for data cleanup
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(
            IServiceProvider serviceProvider,
            ILogger<DataCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting data cleanup job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    // Clean up old integration logs (older than 6 months)
                    await CleanupIntegrationLogsAsync(scope);

                    // Clean up temporary files
                    await CleanupTemporaryFilesAsync(scope);

                    // Clean up old audit logs
                    await CleanupAuditLogsAsync(scope);

                    _logger.LogDebug("Completed data cleanup");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during data cleanup");
                }

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

        private async Task CleanupIntegrationLogsAsync(IServiceScope scope)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
            var cutoffDate = DateTime.UtcNow.AddMonths(-6);

            var oldLogs = dbContext.IntegrationLogs.Where(l => l.CreatedAt < cutoffDate);
            var count = await oldLogs.CountAsync();

            if (count > 0)
            {
                dbContext.IntegrationLogs.RemoveRange(oldLogs);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} old integration logs", count);
            }
        }

        private async Task CleanupTemporaryFilesAsync(IServiceScope scope)
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
                _logger.LogInformation("Cleaned up {Count} temporary files", filesToDelete.Count);
            }
        }

        private async Task CleanupAuditLogsAsync(IServiceScope scope)
        {
            // Placeholder for audit log cleanup
            _logger.LogDebug("Audit log cleanup");
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Background service for backups
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class BackupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupService> _logger;

        public BackupService(
            IServiceProvider serviceProvider,
            ILogger<BackupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting backup job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    // Create database backup
                    await CreateDatabaseBackupAsync(scope);

                    // Backup uploaded files
                    await BackupFilesAsync(scope);

                    _logger.LogInformation("Completed backup");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during backup");
                }

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

        private async Task CreateDatabaseBackupAsync(IServiceScope scope)
        {
            // Placeholder for database backup logic
            _logger.LogInformation("Creating database backup");
            await Task.CompletedTask;
        }

        private async Task BackupFilesAsync(IServiceScope scope)
        {
            // Placeholder for file backup logic
            _logger.LogInformation("Creating file backup");
            await Task.CompletedTask;
        }
    }
}