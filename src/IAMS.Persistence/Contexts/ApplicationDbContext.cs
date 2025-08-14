using IAMS.Domain.Entities;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IAMS.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ITenantContextAccessor? _tenantContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantContextAccessor? tenantContextAccessor = null) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        // DbSets
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


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set audit information and tenant info before saving
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = DateTime.UtcNow;
                        entry.Entity.ModifiedOn = DateTime.UtcNow;

                        // Set tenant ID if applicable and not already set
                        if (entry.Entity.GetType().GetProperty("TenantId") != null &&
                            _tenantContextAccessor?.TenantContext?.Tenant != null)
                        {
                            var tenantIdProperty = entry.Entity.GetType().GetProperty("TenantId");
                            if (tenantIdProperty != null && (int)tenantIdProperty.GetValue(entry.Entity)! == 0)
                            {
                                tenantIdProperty.SetValue(entry.Entity, _tenantContextAccessor.TenantContext.Tenant.Id);
                            }
                        }
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedOn = DateTime.UtcNow;
                        // Prevent modification of CreatedOn and TenantId
                        entry.Property(e => e.CreatedOn).IsModified = false;
                        if (entry.Entity.GetType().GetProperty("TenantId") != null)
                        {
                            entry.Property("TenantId").IsModified = false;
                        }
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }
    }
}