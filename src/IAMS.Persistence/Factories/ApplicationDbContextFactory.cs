using System.IO;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace IAMS.Persistence.Factories
{
    /// <summary>
    /// Design-time factory for ApplicationDbContext to support EF Core migrations
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Navigate to API project directory for configuration files
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IAMS.Api");

            // If running from solution root, adjust path
            if (!Directory.Exists(apiProjectPath))
            {
                apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "IAMS.Api");
            }

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create and return context with null tenant accessor (design-time only)
            return new ApplicationDbContext(optionsBuilder.Options, tenantContextAccessor: null);
        }
    }
}
