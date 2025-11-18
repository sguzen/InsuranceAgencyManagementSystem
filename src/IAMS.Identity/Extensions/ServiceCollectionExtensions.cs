using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using IAMS.Identity.Services;
using IAMS.Identity.Authorization;
using IAMS.Domain.Entities; // Add this
using IAMS.Persistence.Contexts; // Add this

namespace IAMS.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // REMOVE: IdentityDbContext registration
            // REMOVE: ITenantContextAccessor registration (already done in MultiTenancy)

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
            .AddEntityFrameworkStores<ApplicationDbContext>() // Changed from IdentityDbContext
            .AddDefaultTokenProviders();

            // Add custom authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());
            });

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddScoped<IAuthorizationHandler, ModuleHandler>();

            // COMMENT OUT for now:
            // services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            // Add Identity Services
            services.AddScoped<IIdentityService, IdentityService>();
            // Use Transient lifetime to avoid DbContext concurrency issues in Blazor Server
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddScoped<ITokenService, TokenService>();

            // Add data seeder
            //services.AddScoped<IdentityDataSeeder>();

            return services;
        }
    }
}