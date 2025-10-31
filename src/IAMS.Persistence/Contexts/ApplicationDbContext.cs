using IAMS.Domain.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly ITenantContextAccessor? _tenantContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ITenantContextAccessor? tenantContextAccessor = null) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        // Business entities
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<PolicyType> PolicyTypes { get; set; }
        public DbSet<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; }
        public DbSet<CommissionRate> CommissionRates { get; set; }
        public DbSet<PolicyPayment> PolicyPayments { get; set; }
        public DbSet<PolicyClaim> PolicyClaims { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Subdistrict> Subdistricts { get; set; }
        public DbSet<Village> Villages { get; set; }


        // Identity entities (inherited from IdentityDbContext)
        // Users, Roles, UserRoles, etc. are already included

        // Custom Identity entities
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply business entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure decimal precision globally
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // Identity table renaming
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Module).HasMaxLength(100);
                entity.HasIndex(p => p.Name).IsUnique();
            });

            // Configure RolePermission entity (many-to-many)
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.Permissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Additional configurations for ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Additional configurations for ApplicationRole
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(r => r.Description).HasMaxLength(500);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Brand)
            .WithMany(b => b.Vehicles)
            .HasForeignKey(v => v.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Model)
                .WithMany(m => m.Vehicles)
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<VehicleModel>()
                .HasOne(m => m.Brand)
                .WithMany(b => b.Models)
                .HasForeignKey(m => m.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PolicyPayment>()
                .HasOne(m => m.Currency)
                .WithMany(c => c.PolicyPayments)
                .OnDelete(DeleteBehavior.Restrict);

            ParametricDataSeeder.SeedParametricData(modelBuilder);

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set audit information before saving
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = DateTime.UtcNow;
                        entry.Entity.ModifiedOn = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedOn = DateTime.UtcNow;
                        entry.Property(e => e.CreatedOn).IsModified = false;
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