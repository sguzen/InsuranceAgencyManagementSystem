using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IAMS.Identity.Contexts;
using IAMS.Identity.Models;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IAMS.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Identity DbContext
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

            // Add Identity
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

            // Add Identity Services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IPermissionService, PermissionService>();

            return services;
        }
    }
}