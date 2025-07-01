using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.UnitOfWork;
using IAMS.Application.Interfaces.Repositories;

namespace IAMS.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                // Enable sensitive data logging in development
                if (configuration.GetValue<bool>("EnableSensitiveDataLogging"))
                {
                    options.EnableSensitiveDataLogging();
                }

                // Enable detailed errors in development
                if (configuration.GetValue<bool>("EnableDetailedErrors"))
                {
                    options.EnableDetailedErrors();
                }
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Register generic repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register specialized repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
            services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
            services.AddScoped<ICustomerInsuranceCompanyRepository, CustomerInsuranceCompanyRepository>();
            services.AddScoped<IPolicyPaymentRepository, PolicyPaymentRepository>();
            services.AddScoped<IPolicyClaimRepository, PolicyClaimRepository>();
            services.AddScoped<ICommissionRateRepository, CommissionRateRepository>();

            return services;
        }
    }
}