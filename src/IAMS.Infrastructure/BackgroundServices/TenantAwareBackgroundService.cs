using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Example background service for policy reminders
    /// Note: No longer uses multi-tenancy context. Each tenant instance runs its own services against their own database.
    /// </summary>
    public class PolicyReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PolicyReminderService> _logger;

        public PolicyReminderService(
            IServiceProvider serviceProvider,
            ILogger<PolicyReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting policy reminder job");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var policyService = scope.ServiceProvider.GetRequiredService<IPolicyService>();

                    // Process expiring policies - operates on current database context
                    await policyService.ProcessExpiringPoliciesAsync();

                    _logger.LogDebug("Processed policy reminders");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing policy reminders");
                }

                _logger.LogInformation("Completed policy reminder job");

                // Wait 24 hours before next execution
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}