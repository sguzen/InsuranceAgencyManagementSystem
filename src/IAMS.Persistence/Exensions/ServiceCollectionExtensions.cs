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
                var tenantContextAccessor = serviceProvider.GetService<ITenantContextAccessor>();
                var connectionString = tenantContextAccessor?.GetConnectionString()
                    ?? configuration.GetConnectionString("DefaultConnection");

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