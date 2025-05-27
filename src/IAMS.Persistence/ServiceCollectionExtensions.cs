using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IAMS.Application.Interfaces;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.UnitOfWork;

namespace IAMS.Persistence
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
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register specialized repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            // Add other specialized repositories as needed

            return services;
        }
    }
}
