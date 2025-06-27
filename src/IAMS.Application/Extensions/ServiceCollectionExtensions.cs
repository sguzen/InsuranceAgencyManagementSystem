using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using IAMS.Application.Services.Customers;
using IAMS.Application.Services.Policies;
using IAMS.Application.Validators.Customer;
using IAMS.Application.Validators.Policy;
using System.Reflection;

namespace IAMS.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register Application Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPolicyService, PolicyService>();

            return services;
        }
    }
}