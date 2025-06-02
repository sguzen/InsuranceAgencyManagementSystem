using IAMS.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;

namespace IAMS.MultiTenancy.Models
{
    public class TenantContext : DbContext
    {
        public TenantContext(DbContextOptions<TenantContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<TenantModule> TenantModules { get; set; }
        public DbSet<TenantSetting> TenantSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TenantEntity
            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Identifier).IsUnique();
                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.TimeZone).HasMaxLength(50).HasDefaultValue("UTC");
                entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
                entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("en");
                entity.Property(e => e.SubscriptionPlan).HasMaxLength(50);
                entity.Property(e => e.MaxUsers).HasDefaultValue(10);
                entity.Property(e => e.MaxStorageBytes).HasDefaultValue(1073741824L); // 1GB
            });

            // Configure TenantModule
            modelBuilder.Entity<TenantModule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.ModuleName }).IsUnique();

                entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.TenantModules)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TenantSetting
            modelBuilder.Entity<TenantSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.SettingKey }).IsUnique();

                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingValue).IsRequired();
                entity.Property(e => e.SettingType).HasMaxLength(50).HasDefaultValue("string");

                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.TenantSettings)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default data
            SeedDefaultData(modelBuilder);
        }

        private void SeedDefaultData(ModelBuilder modelBuilder)
        {
            // Seed default tenant for development
            modelBuilder.Entity<TenantEntity>().HasData(
                new TenantEntity
                {
                    Id = 1,
                    Name = "Default Insurance Agency",
                    Identifier = "default",
                    ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=IAMS_Default;Trusted_Connection=true;MultipleActiveResultSets=true",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    SubscriptionPlan = "Basic",
                    MaxUsers = 10,
                    MaxStorageBytes = 1073741824L, // 1GB
                    ContactEmail = "admin@defaultagency.com",
                    TimeZone = "UTC",
                    Currency = "USD",
                    Language = "en"
                }
            );

            // Seed default modules
            var tenantId = 1;
            var modules = new[]
            {
                "Policy Management",
                "Customer Management",
                "Reporting",
                "User Management",
                "Integration",
                "Accounting",
                "Dashboard"
            };

            var tenantModules = modules.Select((module, index) => new TenantModule
            {
                Id = index + 1,
                TenantId = tenantId,
                ModuleName = module,
                IsEnabled = true,
                CreatedOn = DateTime.UtcNow
            });

            modelBuilder.Entity<TenantModule>().HasData(tenantModules);

            // Seed default settings
            var tenantSettings = new[]
            {
                new TenantSetting { Id = 1, TenantId = tenantId, SettingKey = "Theme", SettingValue = "Light", SettingType = "string", CreatedOn = DateTime.UtcNow },
                new TenantSetting { Id = 2, TenantId = tenantId, SettingKey = "ItemsPerPage", SettingValue = "25", SettingType = "int", CreatedOn = DateTime.UtcNow },
                new TenantSetting { Id = 3, TenantId = tenantId, SettingKey = "AutoBackup", SettingValue = "true", SettingType = "bool", CreatedOn = DateTime.UtcNow },
                new TenantSetting { Id = 4, TenantId = tenantId, SettingKey = "NotificationEmail", SettingValue = "true", SettingType = "bool", CreatedOn = DateTime.UtcNow }
            };

            modelBuilder.Entity<TenantSetting>().HasData(tenantSettings);
        }
    }
}