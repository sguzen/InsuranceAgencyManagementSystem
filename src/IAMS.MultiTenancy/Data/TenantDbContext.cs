using Microsoft.EntityFrameworkCore;
using IAMS.MultiTenancy.Entities;

namespace IAMS.MultiTenancy.Data
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
        {
        }

        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<TenantModule> TenantModules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TenantEntity
            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Identifier).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
            });

            // Configure TenantModule
            modelBuilder.Entity<TenantModule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.ModuleName }).IsUnique();
                entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Use static date instead of DateTime.UtcNow
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed a default tenant for development
            modelBuilder.Entity<TenantEntity>().HasData(
                new TenantEntity
                {
                    Id = 1,
                    Name = "Default Insurance Agency",
                    Identifier = "default",
                    ConnectionString = "Data Source=localhost;Initial Catalog=TenantDb;Integrated Security=True;Trust Server Certificate=True",
                    IsActive = true,
                    CreatedOn = seedDate, // Static date instead of DateTime.UtcNow
                    SubscriptionPlan = "Premium",
                    MaxUsers = 50,
                    MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5GB
                    ContactEmail = "admin@default-agency.com",
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr"
                }
            );

            // Seed default modules with static dates
            modelBuilder.Entity<TenantModule>().HasData(
                new TenantModule
                {
                    Id = 1,
                    TenantId = 1,
                    ModuleName = "Policy",
                    IsEnabled = true,
                    CreatedOn = seedDate // Static date instead of DateTime.UtcNow
                },
                new TenantModule
                {
                    Id = 2,
                    TenantId = 1,
                    ModuleName = "Customer",
                    IsEnabled = true,
                    CreatedOn = seedDate
                },
                new TenantModule
                {
                    Id = 3,
                    TenantId = 1,
                    ModuleName = "Reporting",
                    IsEnabled = true,
                    CreatedOn = seedDate
                },
                new TenantModule
                {
                    Id = 4,
                    TenantId = 1,
                    ModuleName = "Accounting",
                    IsEnabled = true,
                    CreatedOn = seedDate
                },
                new TenantModule
                {
                    Id = 5,
                    TenantId = 1,
                    ModuleName = "Integration",
                    IsEnabled = true,
                    CreatedOn = seedDate
                }
            );
        }
    }
}