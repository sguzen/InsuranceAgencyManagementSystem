using Microsoft.Extensions.DependencyInjection;
using IAMS.Application.Services.Customers;
using System.Reflection;

namespace IAMS.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Add AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Add Application Services
            services.AddScoped<ICustomerService, CustomerService>();

            return services;
        }
    }
}
