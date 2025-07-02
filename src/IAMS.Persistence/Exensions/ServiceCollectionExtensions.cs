// IAMS.Persistence/Extensions/ServiceCollectionExtensions.cs
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
            // Add tenant-aware DbContext factory
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                // Try to get tenant context accessor, but handle the case where it might not be available
                var tenantContextAccessor = serviceProvider.GetService<ITenantContextAccessor>();

                string connectionString;
                if (tenantContextAccessor?.TenantContext?.Tenant != null)
                {
                    // Use tenant-specific connection string
                    connectionString = tenantContextAccessor.TenantContext.Tenant.ConnectionString;
                }
                else
                {
                    // Fallback to default connection string
                    connectionString = configuration.GetConnectionString("DefaultConnection");
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "No connection string available for ApplicationDbContext. " +
                        "Please ensure DefaultConnection is configured or tenant context is properly set.");
                }

                options.UseSqlServer(connectionString);
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