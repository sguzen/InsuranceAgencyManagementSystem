using IAMS.MultiTenancy.Data;
using IAMS.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IAMS.IntegrationTests.Fixtures;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<TenantDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add test databases
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestApplicationDb");
            });

            services.AddDbContext<TenantDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestMasterDb");
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain references to the database contexts
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var appDb = scopedServices.GetRequiredService<ApplicationDbContext>();
            var masterDb = scopedServices.GetRequiredService<TenantDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

            try
            {
                // Ensure the databases are created
                appDb.Database.EnsureCreated();
                masterDb.Database.EnsureCreated();

                // Seed test data
                DatabaseSeeder.SeedTestData(appDb, masterDb);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the test database.");
                throw;
            }
        });

        builder.UseEnvironment("Testing");
    }
}