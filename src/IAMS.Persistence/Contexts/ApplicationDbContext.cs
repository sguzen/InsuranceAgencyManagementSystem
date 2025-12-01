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
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Marketer> Marketers { get; set; }

        // Tenant-specific configuration settings
        public DbSet<TenantSettings> TenantSettings { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Subdistrict> Subdistricts { get; set; }
        public DbSet<Village> Villages { get; set; }

        // Vehicle Management
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }

        // Currency Management
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }

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

            // Configure concurrency control (RowVersion) for all BaseEntity entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<byte[]>("RowVersion")
                    .IsRowVersion()
                    .HasColumnName("RowVersion");
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

            // Configure TenantSettings entity
            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(s => s.SettingValue).IsRequired();
                entity.Property(s => s.Category).HasMaxLength(50);
                entity.Property(s => s.Description).HasMaxLength(500);
                entity.HasIndex(s => s.SettingKey).IsUnique();
                entity.HasIndex(s => s.Category);
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

            // Configure CurrencyExchangeRate to prevent multiple cascade paths
            // SQL Server doesn't allow cascade delete on both FK relationships to the same table
            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(e => e.FromCurrency)
                .WithMany()
                .HasForeignKey(e => e.FromCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(e => e.ToCurrency)
                .WithMany()
                .HasForeignKey(e => e.ToCurrencyId)
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
                        // For new entities, always set timestamps if not already set
                        if (entry.Entity.CreatedOn == default || entry.Entity.CreatedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.CreatedOn = DateTime.UtcNow;
                        }
                        if (entry.Entity.ModifiedOn == null || entry.Entity.ModifiedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.ModifiedOn = DateTime.UtcNow;
                        }
                        break;

                    case EntityState.Modified:
                        // For modified entities, only update ModifiedOn if it hasn't been recently set by the handler
                        // Check if ModifiedOn was explicitly set by comparing with a recent timeframe
                        var currentModifiedOn = entry.Entity.ModifiedOn;
                        if (currentModifiedOn == null || currentModifiedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.ModifiedOn = DateTime.UtcNow;
                        }
                        // Never modify CreatedOn for existing entities
                        entry.Property(e => e.CreatedOn).IsModified = false;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // Set audit information before saving
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // For new entities, always set timestamps if not already set
                        if (entry.Entity.CreatedOn == default || entry.Entity.CreatedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.CreatedOn = DateTime.UtcNow;
                        }
                        if (entry.Entity.ModifiedOn == null || entry.Entity.ModifiedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.ModifiedOn = DateTime.UtcNow;
                        }
                        break;

                    case EntityState.Modified:
                        // For modified entities, only update ModifiedOn if it hasn't been recently set by the handler
                        var currentModifiedOn = entry.Entity.ModifiedOn;
                        if (currentModifiedOn == null || currentModifiedOn < DateTime.UtcNow.AddSeconds(-10))
                        {
                            entry.Entity.ModifiedOn = DateTime.UtcNow;
                        }
                        // Never modify CreatedOn for existing entities
                        entry.Property(e => e.CreatedOn).IsModified = false;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}