// src/IAMS.Application/Extensions/ServiceCollectionExtensions.cs (FINAL UPDATE)
using Microsoft.Extensions.DependencyInjection;
using IAMS.Application.Services.Customers;
using IAMS.Application.Services.Policies;
using IAMS.Application.Services.InsuranceCompanies;
using IAMS.Application.Services.CustomerMappings;
using IAMS.Application.Services.PolicyTypes;
using IAMS.Application.Behaviors;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace IAMS.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Add AutoMapper
            services.AddAutoMapper(assembly);

            // Add MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // Add MediatR Pipeline Behaviors
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            // Add FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Add Application Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();
            services.AddScoped<ICustomerMappingService, CustomerMappingService>();
            services.AddScoped<IPolicyTypeService, PolicyTypeService>();

            return services;
        }
    }
}