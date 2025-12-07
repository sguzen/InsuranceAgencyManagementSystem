// IAMS.Persistence/Extensions/ServiceCollectionExtensions.cs
using IAMS.Shared.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.Services;
using IAMS.Persistence.UnitOfWork;
using IAMS.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ITenantService = IAMS.Shared.Interfaces.ITenantService;

namespace IAMS.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register ApplicationDbContext with TENANT-SPECIFIC connection string
            // Use Scoped lifetime (standard for DbContext)
            // For Blazor Server threading, use OwningComponentBase<T> in components
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var tenantContextAccessor = serviceProvider.GetService<ITenantContextAccessor>();

                string connectionString;

                // Try to get tenant-specific connection string
                if (tenantContextAccessor?.TenantContext?.Tenant != null)
                {
                    connectionString = tenantContextAccessor.GetConnectionString();

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException(
                            $"No connection string found for tenant '{tenantContextAccessor.TenantContext.TenantIdentifier}'");
                    }
                }
                else
                {
                    // Fallback to default connection (for migrations, seeding, etc.)
                    connectionString = configuration.GetConnectionString(ApplicationConstants.ConnectionStrings.DefaultConnection);

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException(
                            $"{ApplicationConstants.ConnectionStrings.DefaultConnection} string is required when no tenant context is available. " +
                            "Please ensure it's configured in appsettings.json");
                    }
                }

                options.UseSqlServer(connectionString);

                // Optional: Add logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            }); // Default is Scoped lifetime

            // Register generic repository and unit of work
            // Use scoped to match DbContext lifetime for proper Unit of Work pattern
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();

            // Register specialized repositories as scoped
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
            services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<ICommissionRateRepository, CommissionRateRepository>();

            // Register tenant service (moved from Infrastructure to fix dependency direction)
            // This service uses ApplicationDbContext directly, so it belongs in Persistence layer
            services.AddScoped<ITenantService, ApplicationTenantService>();

            return services;
        }
    }
}