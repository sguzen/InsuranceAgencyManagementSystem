using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.BackgroundServices
{
    public abstract class TenantAwareBackgroundService : BackgroundService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        protected TenantAwareBackgroundService(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected async Task ExecuteForAllTenantsAsync(Func<Tenant, IServiceScope, Task> operation)
        {
            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();

            // Get all active tenants
            var tenants = await GetActiveTenantsAsync(tenantService);

            foreach (var tenant in tenants)
            {
                try
                {
                    await tenantContextAccessor.ExecuteWithTenantAsync(tenant, async () =>
                    {
                        using var tenantScope = _serviceProvider.CreateScope();
                        await operation(tenant, tenantScope);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tenant {TenantId} in background service", tenant.Id);
                }
            }
        }

        protected async Task ExecuteForTenantAsync(int tenantId, Func<Tenant, IServiceScope, Task> operation)
        {
            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();

            var tenant = await tenantService.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found for background operation", tenantId);
                return;
            }

            try
            {
                await tenantContextAccessor.ExecuteWithTenantAsync(tenant, async () =>
                {
                    using var tenantScope = _serviceProvider.CreateScope();
                    await operation(tenant, tenantScope);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tenant {TenantId} in background service", tenant.Id);
            }
        }

        private async Task<List<Tenant>> GetActiveTenantsAsync(ITenantService tenantService)
        {
            // This would require adding a method to ITenantService to get all active tenants
            // For now, this is a placeholder implementation
            var tenants = new List<Tenant>();

            // You would implement this method in TenantService to query all active tenants
            // return await tenantService.GetAllActiveTenantsAsync();

            return tenants;
        }
    }

    // Example background service
    public class PolicyReminderService : TenantAwareBackgroundService
    {
        public PolicyReminderService(
            IServiceProvider serviceProvider,
            ILogger<PolicyReminderService> logger) : base(serviceProvider, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting policy reminder job");

                await ExecuteForAllTenantsAsync(async (tenant, scope) =>
                {
                    var policyService = scope.ServiceProvider.GetRequiredService<IPolicyService>();

                    // Process expiring policies for this tenant
                    await policyService.ProcessExpiringPoliciesAsync();

                    _logger.LogDebug("Processed policy reminders for tenant {TenantId}", tenant.Id);
                });

                _logger.LogInformation("Completed policy reminder job");

                // Wait 24 hours before next execution
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}