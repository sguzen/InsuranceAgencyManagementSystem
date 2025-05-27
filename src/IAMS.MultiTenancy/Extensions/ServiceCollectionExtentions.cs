using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAMS.MultiTenancy.Services;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            // Register tenant context as scoped so it's available per request
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantService, TenantService>();

            return services;
        }
    }
}