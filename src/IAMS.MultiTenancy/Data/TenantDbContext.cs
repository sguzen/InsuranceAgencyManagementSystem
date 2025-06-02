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
        public DbSet<TenantSetting> TenantSettings { get; set; }

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

            // Configure TenantSetting
            modelBuilder.Entity<TenantSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.SettingKey }).IsUnique();
                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingValue).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.SettingType).HasMaxLength(50);

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
            // Seed a default tenant for development
            modelBuilder.Entity<TenantEntity>().HasData(
                new TenantEntity
                {
                    Id = 1,
                    Name = "Default Insurance Agency",
                    Identifier = "default",
                    ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=IAMS_Default;Trusted_Connection=true;MultipleActiveResultSets=true",
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    SubscriptionPlan = "Premium",
                    MaxUsers = 50,
                    MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5GB
                    ContactEmail = "admin@default-agency.com",
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr"
                }
            );

            // Seed default modules
            var moduleNames = new[] { "Policy", "Customer", "Reporting", "Accounting", "Integration" };
            var moduleData = new List<TenantModule>();

            for (int i = 0; i < moduleNames.Length; i++)
            {
                moduleData.Add(new TenantModule
                {
                    Id = i + 1,
                    TenantId = 1,
                    ModuleName = moduleNames[i],
                    IsEnabled = true,
                    CreatedOn = DateTime.UtcNow
                });
            }

            modelBuilder.Entity<TenantModule>().HasData(moduleData);
        }
    }
}