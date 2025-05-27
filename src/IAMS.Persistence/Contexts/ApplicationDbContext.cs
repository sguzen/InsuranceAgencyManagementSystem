using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<PolicyType> PolicyTypes { get; set; }
        public DbSet<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; }
        public DbSet<CommissionRate> CommissionRates { get; set; }
        public DbSet<PolicyPayment> PolicyPayments { get; set; }
        public DbSet<PolicyClaim> PolicyClaims { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure decimal precision globally
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}