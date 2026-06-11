using IAMS.MultiTenancy.Data;
using IAMS.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;

namespace IAMS.IntegrationTests.Fixtures;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=IAMS_Test;Integrated Security=true;TrustServerCertificate=true;" },
                { "ConnectionStrings:MasterConnection", "Server=localhost;Database=IAMS_Master_Test;Integrated Security=true;TrustServerCertificate=true;" },
                { "JwtSettings:Secret", "test-secret-key-for-testing-only-not-for-production-needs-to-be-32-chars-long" },
                { "JwtSettings:Issuer", "IAMS-Test" },
                { "JwtSettings:Audience", "IAMS-Test-Users" },
                { "ApiSettings:ApiKey", "test-api-key-12345" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations and any DbConnection
            var descriptors = services.Where(d =>
                d.ServiceType.Name.Contains("DbContextOptions") ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                d.ServiceType == typeof(TenantDbContext) ||
                d.ServiceType == typeof(IAMS.Infrastructure.Data.IntegrationDbContext) ||
                d.ServiceType.Name.Contains("DbConnection"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Mock ITenantDatabaseService to prevent it from manually creating SQL Server connections
            var tenantDbServiceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAMS.Shared.Interfaces.ITenantDatabaseService));
            if (tenantDbServiceDescriptor != null)
            {
                services.Remove(tenantDbServiceDescriptor);
            }
            
            var mockTenantDbService = new Moq.Mock<IAMS.Shared.Interfaces.ITenantDatabaseService>();
            mockTenantDbService.Setup(m => m.EnsureTenantDatabaseAsync(Moq.It.IsAny<string>())).Returns(Task.CompletedTask);
            mockTenantDbService.Setup(m => m.CreateTenantDatabaseAsync(Moq.It.IsAny<string>())).Returns(Task.CompletedTask);
            services.AddScoped(_ => mockTenantDbService.Object);

            // Add test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                "Test", options => { });

            services.Configure<Microsoft.AspNetCore.Authorization.AuthorizationOptions>(options =>
            {
                options.AddPolicy("ApiKeyOrJwt", policy =>
                {
                    policy.AddAuthenticationSchemes("Test");
                    policy.RequireAuthenticatedUser();
                });
            });

            // Add test databases
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestApplicationDb");
            });

            services.AddDbContext<TenantDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestMasterDb");
            });
            
            services.AddDbContext<IAMS.Infrastructure.Data.IntegrationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestIntegrationDb");
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