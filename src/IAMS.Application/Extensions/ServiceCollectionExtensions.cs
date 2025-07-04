using FluentValidation;
using IAMS.Application.Behaviors;
using IAMS.Application.Services;
using IAMS.Application.Services.Customers;
using IAMS.Application.Services.InsuranceCompanies;
using IAMS.Application.Services.Policies;
using IAMS.Application.Validators.Customer;
using IAMS.Application.Validators.Policy;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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

            // Register Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            // Register Application Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();
            services.AddScoped<ICustomerCodeGenerator, CustomerCodeGenerator>();


            return services;
        }
    }
}