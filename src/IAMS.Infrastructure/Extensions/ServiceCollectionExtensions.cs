// IAMS.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IAMS.Infrastructure.Data;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Services;
using IAMS.Infrastructure.BackgroundServices;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Services;

namespace IAMS.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Infrastructure DbContext
            services.AddDbContext<IntegrationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add HTTP Client Factory
            services.AddHttpClient();

            // Add Core Infrastructure Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IReportingService, ReportingService>();

            // Add File Storage Service (configurable)
            var storageType = configuration["FileStorage:Type"]?.ToLower() ?? "local";
            switch (storageType)
            {
                case "azure":
                    services.AddScoped<IFileStorageService, AzureBlobStorageService>();
                    break;
                case "local":
                default:
                    services.AddScoped<IFileStorageService, LocalFileStorageService>();
                    break;
            }

            // Add Policy Service (this would be implemented in your business layer)
            services.AddScoped<IPolicyService, PolicyService>();

            // Add Background Services
            services.AddHostedService<PolicyReminderService>();
            services.AddHostedService<IntegrationSyncService>();
            services.AddHostedService<ReportSchedulerService>();

            // Add Module Service for feature toggles
            services.AddScoped<IModuleService, ModuleService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureBackgroundServices(this IServiceCollection services)
        {
            // Additional background services for insurance operations
            services.AddHostedService<ClaimProcessingService>();
            services.AddHostedService<DataCleanupService>();
            services.AddHostedService<BackupService>();

            return services;
        }
    }
}