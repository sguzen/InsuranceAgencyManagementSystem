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
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add repositories and unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }
    }
}