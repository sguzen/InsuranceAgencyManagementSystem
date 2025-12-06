using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Services;
using IAMS.Infrastructure.Data;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Services;
using IAMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAMS.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Integration Database for logs, reports, and file metadata
            services.AddDbContext<IntegrationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.IntegrationConnection)
                    ?? throw new InvalidOperationException($"{ApplicationConstants.ConnectionStrings.IntegrationConnection} is required")));

            // Add HTTP Client Factory for integration services
            services.AddHttpClient();

            // Register infrastructure services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IVehicleDataService, VehicleDataService>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IExternalPolicyImportService, ExternalPolicyImportService>();

            // Configure email settings
            services.Configure<EmailSettings>(configuration.GetSection(ApplicationConstants.ConfigurationSections.EmailSettings));

            // Configure file storage settings
            services.Configure<FileStorageSettings>(configuration.GetSection(ApplicationConstants.ConfigurationSections.FileStorageSettings));

            return services;
        }

        public static IServiceCollection AddInfrastructureBackgroundServices(
            this IServiceCollection services)
        {
            // Register background services - only register ones that exist
            // Commenting out missing ones for now - uncomment as you create them
            // services.AddHostedService<PolicyReminderService>();
            // services.AddHostedService<IntegrationSyncService>();
            // services.AddHostedService<ReportSchedulerService>();
            // services.AddHostedService<ClaimProcessingService>();
            // services.AddHostedService<DataCleanupService>();
            // services.AddHostedService<BackupService>();

            return services;
        }
    }

    // Configuration classes
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class FileStorageSettings
    {
        public string StorageType { get; set; } = "Local"; // Local, Azure, AWS
        public string LocalPath { get; set; } = "uploads";
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}