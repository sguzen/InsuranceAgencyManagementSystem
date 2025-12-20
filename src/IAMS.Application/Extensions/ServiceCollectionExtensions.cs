using FluentValidation;
using IAMS.Application.Behaviors;
using IAMS.Application.Interfaces;
using IAMS.Application.Services;
using IAMS.Application.Services.Claims;
using IAMS.Application.Services.Currencies;
using IAMS.Application.Services.Customers;
using IAMS.Application.Services.InsuranceCompanies;
using IAMS.Application.Services.Parametric;
using IAMS.Application.Services.Payments;
using IAMS.Application.Services.PolicyTypes;
using IAMS.Application.Services.Vehicles;
using IAMS.Application.Services.Calculations;
using IAMS.Application.Services.CustomerMappings;
using IAMS.Application.Services.PolicyImport;
using IAMS.Application.Validators.Customer;
using IAMS.Application.Validators.Policy;
using IAMS.Domain.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using IAMS.Application.Services.Policies;
using IAMS.Application.Interfaces.Services;

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
            services.AddScoped<IParametricService, ParametricService>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IPolicyTypeService, PolicyTypeService>();
            services.AddScoped<IPolicyNumberGenerator, PolicyNumberGenerator>();
            services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();
            services.AddScoped<ICustomerCodeGenerator, CustomerCodeGenerator>();

            // Register new services
            services.AddScoped<IClaimService, ClaimService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<PaymentAllocationService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IUserManagementService, UserManagementService>();

            // Register Calculation Services
            services.AddScoped<ICommissionCalculator, CommissionCalculator>();
            services.AddScoped<IClaimCalculator, ClaimCalculator>();
            services.AddScoped<IPolicyCalculatorFactory, PolicyCalculatorFactory>();

            // Register Premium Calculators
            services.AddScoped<IPolicyPremiumCalculator, TrafficInsurancePremiumCalculator>();
            services.AddScoped<IPolicyPremiumCalculator, KaskoInsurancePremiumCalculator>();
            services.AddScoped<IPolicyPremiumCalculator, PropertyInsurancePremiumCalculator>();
            services.AddScoped<IPolicyPremiumCalculator, HealthInsurancePremiumCalculator>();
            services.AddScoped<IPolicyPremiumCalculator, LifeInsurancePremiumCalculator>();
            services.AddScoped<IPolicyPremiumCalculator, LiabilityInsurancePremiumCalculator>();

            // Register External Premium Calculation Service with HttpClient
            services.AddHttpClient<IExternalPremiumCalculationService, ExternalPremiumCalculationService>();

            // Register Policy Import Services
            services.AddScoped<IExcelFileValidator, ExcelFileValidator>();
            services.AddScoped<IExcelPolicyParser, ExcelPolicyParser>();

            // Register Focused Policy Services (replaces bloated repository interface)
            services.AddScoped<IPolicyQueryService, PolicyQueryService>();
            services.AddScoped<IPolicyAnalyticsService, PolicyAnalyticsService>();
            services.AddScoped<IPolicyReportingService, PolicyReportingService>();
            services.AddScoped<IPolicyIntegrationService, PolicyIntegrationService>();

            return services;
        }

        /// <summary>
        /// Registers premium calculation configuration
        /// </summary>
        public static IServiceCollection AddPremiumCalculationConfiguration(
            this IServiceCollection services,
            Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            services.Configure<IAMS.Application.Configuration.PremiumCalculationSettings>(
                configuration.GetSection("PremiumCalculation"));

            return services;
        }
    }
}