using FluentValidation;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Services.Claims;
using IAMS.Application.Services.Customers;
using IAMS.Application.Services.InsuranceCompanies;
using IAMS.Application.Services.Payments;
using IAMS.Infrastructure.Data;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Services;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Services;
using IAMS.Persistence.Repositories;
using IAMS.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IModuleService = IAMS.Application.Interfaces.IModuleService;

namespace IAMS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IInsuranceCompanyRepository, InsuranceCompanyRepository>();
        //services.AddScoped<IClaimRepository, ClaimRepository>();
        //services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        //services.AddScoped<IPaymentRepository, PaymentRepository>();
        //services.AddScoped<ICommissionRepository, CommissionRepository>();

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IIntegrationService, IntegrationService>();
        services.AddScoped<IReportingService, ReportingService>();

        return services;
    }

    public static IServiceCollection AddMultiTenancyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register multi-tenancy services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<IModuleService, Services.ModuleService>();

        // Register HTTP context accessor for tenant resolution
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ApplicationMappingProfile));

        // Register application services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICommissionService, CommissionService>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<ApplicationMappingProfile>();

        return services;
    }
}