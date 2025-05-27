using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IAMS.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add infrastructure services here
            // Email services, file storage, external API clients, etc.

            return services;
        }
    }
}