using IAMS.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IAMS.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services)
        {
            // Register Blazor-specific services
            //services.AddScoped<ICustomerComponentService, CustomerComponentService>();
            //services.AddScoped<IPolicyComponentService, PolicyComponentService>();
            //services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IJSInteropService, JSInteropService>();

            // Register Web layer AutoMapper profiles (for infrastructure-dependent mappings)
            // Use explicit array to avoid ambiguity between extension method overloads
            services.AddAutoMapper(new[] { typeof(ServiceCollectionExtensions).Assembly });

            return services;
        }
    }
}