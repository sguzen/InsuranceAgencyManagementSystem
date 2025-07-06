// IAMS.Persistence/Extensions/ServiceCollectionExtensions.cs - FIXED VERSION
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.UnitOfWork;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Persistence.Contexts;
using IAMS.MultiTenancy.Interfaces;

namespace IAMS.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register ApplicationDbContext with FIXED connection string
            // Always use AppDb for shared customer data with tenant isolation via TenantId column
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                // ALWAYS use the default connection string for ApplicationDbContext
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "DefaultConnection string is required for ApplicationDbContext. " +
                        "Please ensure it's configured in appsettings.json");
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

            // Register specialized repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
            //services.AddScoped<IClaimRepository, ClaimRepository>();
            //services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            //services.AddScoped<IPaymentRepository, PaymentRepository>();
            //services.AddScoped<ICommissionRepository, CommissionRepository>();

            return services;
        }
    }
}