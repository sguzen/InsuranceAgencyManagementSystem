using IAMS.Domain.Entities;
using IAMS.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace IAMS.Web.Extensions
{
    public static class DatabaseInitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Get Identity managers
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                // Seed roles and permissions
                await IdentityDataSeeder.SeedRolesAndPermissionsAsync(roleManager, logger);

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
            }
        }
    }
}
