using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Services;
using IAMS.MultiTenancy.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IAMS.Shared.Interfaces;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiTenancyServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Master Database (for tenant management) - support both connection string names
            services.AddDbContext<TenantDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("MasterConnection")
                    ?? configuration.GetConnectionString("MasterDatabase");
                options.UseSqlServer(connectionString);
            });

            // Add memory cache for tenant caching
            services.AddMemoryCache();

            // Register multi-tenancy core services
            services.AddScoped<IAMS.MultiTenancy.Interfaces.ITenantService, TenantService>();
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

            // Register module management service
            // Register for both interfaces (MultiTenancy and Application) to support different layers
            services.AddScoped<ModuleService>();
            services.AddScoped<IModuleService>(sp => sp.GetRequiredService<ModuleService>());
            services.AddScoped<IModuleService>(sp => sp.GetRequiredService<ModuleService>());

            services.AddScoped<ICurrentTenantService, CurrentTenantService>();

            // Configure multi-tenancy options
            services.Configure<MultiTenancyOptions>(configuration.GetSection("MultiTenancy"));

            return services;
        }

        // Alternative method name for compatibility with existing file
        public static IServiceCollection AddMultiTenancy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddMultiTenancyServices(configuration);
        }
    }

    public class MultiTenancyOptions
    {
        public TenantResolutionStrategy Strategy { get; set; } = TenantResolutionStrategy.Subdomain;
        public string DefaultTenant { get; set; } = "default";
        public bool EnableTenantCaching { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 30;
    }

    public enum TenantResolutionStrategy
    {
        Subdomain,
        Header,
        Path,
        QueryParameter
    }
}