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
    private readonly string _dbId = Guid.NewGuid().ToString();

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

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var tenantAccessor = sp.GetService<IAMS.MultiTenancy.Interfaces.ITenantContextAccessor>();
                string dbName = $"TestApplicationDb_{_dbId}_default";

                if (tenantAccessor?.TenantContext?.Tenant != null)
                {
                    dbName = $"TestApplicationDb_{_dbId}_{tenantAccessor.TenantContext.Tenant.Identifier}";
                }

                options.UseInMemoryDatabase(dbName);
            });

            services.AddDbContext<TenantDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestMasterDb_{_dbId}");
            });
            
            services.AddDbContext<IAMS.Infrastructure.Data.IntegrationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestIntegrationDb_{_dbId}");
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var masterDb = scopedServices.GetRequiredService<TenantDbContext>();
                masterDb.Database.EnsureCreated();

                var tenantAccessor = scopedServices.GetRequiredService<IAMS.MultiTenancy.Interfaces.ITenantContextAccessor>();

                // Seed default tenant's app db
                tenantAccessor.TenantContext = new IAMS.MultiTenancy.Models.TenantContext(new IAMS.MultiTenancy.Models.Tenant { Identifier = "default" });
                var appDbDefault = scopedServices.GetRequiredService<ApplicationDbContext>();
                appDbDefault.Database.EnsureCreated();
                DatabaseSeeder.SeedTestData(appDbDefault, masterDb);
            }

            using (var scope2 = serviceProvider.CreateScope())
            {
                var scopedServices2 = scope2.ServiceProvider;
                var tenantAccessor2 = scopedServices2.GetRequiredService<IAMS.MultiTenancy.Interfaces.ITenantContextAccessor>();
                tenantAccessor2.TenantContext = new IAMS.MultiTenancy.Models.TenantContext(new IAMS.MultiTenancy.Models.Tenant { Identifier = "test-agency-1" });
                
                var appDbAgency1 = scopedServices2.GetRequiredService<ApplicationDbContext>();
                var masterDb2 = scopedServices2.GetRequiredService<TenantDbContext>();
                appDbAgency1.Database.EnsureCreated();
                DatabaseSeeder.SeedTestData(appDbAgency1, masterDb2);
            }
        });

        builder.UseEnvironment("Testing");
    }
}