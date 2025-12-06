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
            // Try to find configuration file from multiple possible locations
            string configPath = FindConfigurationPath();

            if (string.IsNullOrEmpty(configPath))
            {
                throw new InvalidOperationException(
                    "Could not find appsettings.json. Searched in IAMS.Web, IAMS.Api, and current directory. " +
                    "Please ensure you're running from the solution root or project directory.");
            }

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "DefaultConnection string not found in configuration. " +
                    "Please ensure appsettings.json contains a valid DefaultConnection.");
            }

            // Build DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create and return context with null tenant accessor (design-time only)
            return new ApplicationDbContext(optionsBuilder.Options, tenantContextAccessor: null);
        }

        private string FindConfigurationPath()
        {
            var currentDir = Directory.GetCurrentDirectory();

            // List of possible paths to check for appsettings.json
            var possiblePaths = new[]
            {
                // From IAMS.Persistence directory
                Path.Combine(currentDir, "..", "IAMS.Web"),
                Path.Combine(currentDir, "..", "IAMS.Api"),
                // From solution root
                Path.Combine(currentDir, "src", "IAMS.Web"),
                Path.Combine(currentDir, "src", "IAMS.Api"),
                // From src directory
                Path.Combine(currentDir, "IAMS.Web"),
                Path.Combine(currentDir, "IAMS.Api"),
                // Current directory (in case appsettings.json is copied here)
                currentDir
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                var configFile = Path.Combine(fullPath, "appsettings.json");

                if (File.Exists(configFile))
                {
                    return fullPath;
                }
            }

            return null;
        }
    }
}
