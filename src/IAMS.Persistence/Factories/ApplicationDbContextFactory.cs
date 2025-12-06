using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IAMS.Persistence.Factories
{
    /// <summary>
    /// Design-time factory for ApplicationDbContext to support EF Core migrations
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Use a simple connection string for design-time operations
            // EF Core migrations don't need the actual production connection
            var connectionString = "Data Source=localhost;Initial Catalog=ApplicationDb;Integrated Security=True;Trust Server Certificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Create and return context with null tenant accessor (design-time only)
            return new ApplicationDbContext(optionsBuilder.Options, tenantContextAccessor: null);
        }
    }
}
