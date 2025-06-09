using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAMS.Persistence.Contexts;
using IAMS.Application.Interfaces;
using IAMS.Persistence.Repositories;

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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register generic repository and unit of work
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Register specialized repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();

            // Add other specialized repositories as needed

            return services;
        }
    }
}