// Extensions/ServiceCollectionExtensions.cs - Final Fixed Implementation
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using IAMS.MultiTenancy.Services;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Middleware;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            // Add master database context for tenant management
            services.AddDbContext<TenantContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("TenantConnection")
                    ?? configuration.GetConnectionString("MasterConnection")
                    ?? configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "No connection string found. Please configure 'TenantConnection', 'MasterConnection', or 'DefaultConnection' in appsettings.json");
                }

                options.UseSqlServer(connectionString);
            });

            // Register HTTP context accessor for web scenarios (only if not already registered)
            //if (!services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
            //{
            //    services.AddHttpContextAccessor();
            //}

            // Register tenant context accessor as scoped
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

            // Register tenant services
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IModuleService, ModuleService>();

            // Add memory cache for tenant caching (only if not already registered)
            if (!services.Any(s => s.ServiceType == typeof(IMemoryCache)))
            {
                services.AddMemoryCache();
            }

            return services;
        }

        public static IServiceCollection AddMultiTenancyWithCustomDatabase(
            this IServiceCollection services,
            string masterConnectionString)
        {
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                throw new ArgumentException("Master connection string cannot be null or empty", nameof(masterConnectionString));
            }

            // Add master database context with custom connection string
            services.AddDbContext<TenantContext>(options =>
                options.UseSqlServer(masterConnectionString));

            // Register HTTP context accessor (only if not already registered)
            //if (!services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
            //{
            //    services.AddHttpContextAccessor();
            //}

            // Register services
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IModuleService, ModuleService>();

            // Add memory cache (only if not already registered)
            if (!services.Any(s => s.ServiceType == typeof(IMemoryCache)))
            {
                services.AddMemoryCache();
            }

            return services;
        }

        /// <summary>
        /// Adds multi-tenancy with advanced configuration options
        /// </summary>
        public static IServiceCollection AddMultiTenancy(
            this IServiceCollection services,
            Action<MultiTenancyOptions> configureOptions)
        {
            var options = new MultiTenancyOptions();
            configureOptions(options);

            // Validate options
            if (string.IsNullOrEmpty(options.MasterConnectionString))
            {
                throw new InvalidOperationException("MasterConnectionString is required");
            }

            // Add database context
            services.AddDbContext<TenantContext>(dbOptions =>
            {
                dbOptions.UseSqlServer(options.MasterConnectionString);

                if (options.EnableSensitiveDataLogging)
                {
                    dbOptions.EnableSensitiveDataLogging();
                }

                if (options.EnableDetailedErrors)
                {
                    dbOptions.EnableDetailedErrors();
                }
            });

            // Configure caching
            if (options.EnableCaching)
            {
                if (!services.Any(s => s.ServiceType == typeof(IMemoryCache)))
                {
                    services.AddMemoryCache(cacheOptions =>
                    {
                        cacheOptions.SizeLimit = options.CacheSizeLimit;
                    });
                }
            }

            // Register HTTP context accessor
            //if (!services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
            //{
            //    services.AddHttpContextAccessor();
            //}

            // Register core services
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IModuleService, ModuleService>();

            // Register configuration
            services.AddSingleton(options);

            return services;
        }
    }

    /// <summary>
    /// Configuration options for multi-tenancy
    /// </summary>
    public class MultiTenancyOptions
    {
        public string MasterConnectionString { get; set; } = string.Empty;
        public bool EnableCaching { get; set; } = true;
        public int CacheSizeLimit { get; set; } = 1000;
        public int CacheExpirationMinutes { get; set; } = 30;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = false;
        public TenantResolutionStrategy ResolutionStrategy { get; set; } = TenantResolutionStrategy.Header;
        public string DefaultTenant { get; set; } = "default";
        public string HeaderName { get; set; } = "X-Tenant-ID";
    }

    /// <summary>
    /// Tenant resolution strategies
    /// </summary>
    public enum TenantResolutionStrategy
    {
        Header,
        Subdomain,
        Path,
        QueryParameter
    }

    /// <summary>
    /// Extension methods for configuring multi-tenancy middleware
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds tenant resolution middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantMiddleware>();
        }

        /// <summary>
        /// Adds tenant resolution middleware with performance monitoring
        /// </summary>
        public static IApplicationBuilder UseMultiTenancyWithPerformanceMonitoring(this IApplicationBuilder app)
        {
            return app
                .UseMiddleware<TenantMiddleware>();
            // Note: TenantPerformanceMiddleware would need to be implemented separately
        }
    }
}