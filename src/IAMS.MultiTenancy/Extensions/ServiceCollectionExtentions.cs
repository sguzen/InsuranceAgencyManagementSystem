using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IAMS.MultiTenancy.Services;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Data;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Entity Framework context for tenant management
            services.AddDbContext<TenantContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MasterDatabase")));

            // Register multi-tenancy services
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IModuleService, ModuleService>();

            // Add HTTP context accessor for tenant resolution
            services.AddHttpContextAccessor();

            return services;
        }
    }
}