// IAMS.Persistence/Extensions/ServiceCollectionExtensions.cs
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.Services;
using IAMS.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAMS.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register ApplicationDbContext with TENANT-SPECIFIC connection string
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
                    connectionString = configuration.GetConnectionString("DefaultConnection");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException(
                            "DefaultConnection string is required when no tenant context is available. " +
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
            });

            // Register generic repository and unit of work
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();
            // Register specialized repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();

            return services;
        }
    }
}