// IAMS.Identity/Extensions/ServiceCollectionExtensions.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using IAMS.Identity.Data;
using IAMS.Identity.Models;
using IAMS.Identity.Services;
using IAMS.Identity.Authorization;
using IAMS.Identity.Interfaces;

namespace IAMS.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Identity DbContext
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add Identity with custom user and role
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            // Add custom authorization
            services.AddAuthorization(options =>
            {
                // Add default policies if needed
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddScoped<IAuthorizationHandler, ModuleHandler>();

            // Register custom policy provider
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            // Add Identity Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ITokenService, TokenService>();

            // Add data seeder
            services.AddScoped<IdentityDataSeeder>();

            return services;
        }

        public static IServiceCollection AddTenantContextAccessor(this IServiceCollection services)
        {
            services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
            return services;
        }
    }

    // Implementation of ITenantContextAccessor
    public class TenantContextAccessor : ITenantContextAccessor
    {
        public TenantContext TenantContext { get; private set; }

        public TenantContextAccessor()
        {
            // Initialize with empty context - this should be set by middleware
            TenantContext = new TenantContext();
        }

        public void SetTenantContext(TenantContext context)
        {
            TenantContext = context;
        }
    }
}