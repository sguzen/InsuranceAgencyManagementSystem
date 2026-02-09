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
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<AgencyInsuranceCompany> AgencyInsuranceCompanies { get; set; }
        public DbSet<ImportJob> ImportJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TenantEntity (Agency)
            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Identifier).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ModuleSettings).HasMaxLength(4000);
                entity.Property(e => e.Settings).HasMaxLength(4000);
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

            // Configure InsuranceCompany
            modelBuilder.Entity<InsuranceCompany>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            });

            // Configure AgencyInsuranceCompany
            modelBuilder.Entity<AgencyInsuranceCompany>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AgencyId, e.InsuranceCompanyId }).IsUnique();

                entity.HasOne(e => e.Agency)
                    .WithMany(a => a.AgencyInsuranceCompanies)
                    .HasForeignKey(e => e.AgencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InsuranceCompany)
                    .WithMany(ic => ic.AgencyInsuranceCompanies)
                    .HasForeignKey(e => e.InsuranceCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ImportJob
            modelBuilder.Entity<ImportJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.AgencyId);
                entity.HasIndex(e => e.InsuranceCompanyId);
                entity.Property(e => e.RequestedBy).HasMaxLength(256);
                entity.Property(e => e.CreatedBy).HasMaxLength(256);
                entity.Property(e => e.ModifiedBy).HasMaxLength(256);

                entity.HasOne(e => e.Agency)
                    .WithMany()
                    .HasForeignKey(e => e.AgencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InsuranceCompany)
                    .WithMany()
                    .HasForeignKey(e => e.InsuranceCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed default data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Use static date instead of DateTime.UtcNow
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Turkish insurance companies
            modelBuilder.Entity<InsuranceCompany>().HasData(
                new InsuranceCompany { Id = 1, Name = "Anadolu Sigorta", Code = "ANADOLU", IsActive = true, DisplayOrder = 1, CreatedOn = seedDate },
                new InsuranceCompany { Id = 2, Name = "Allianz Sigorta", Code = "ALLIANZ", IsActive = true, DisplayOrder = 2, CreatedOn = seedDate },
                new InsuranceCompany { Id = 3, Name = "Axa Sigorta", Code = "AXA", IsActive = true, DisplayOrder = 3, CreatedOn = seedDate },
                new InsuranceCompany { Id = 4, Name = "Aksigorta", Code = "AKSIGORTA", IsActive = true, DisplayOrder = 4, CreatedOn = seedDate },
                new InsuranceCompany { Id = 5, Name = "Sompo Sigorta", Code = "SOMPO", IsActive = true, DisplayOrder = 5, CreatedOn = seedDate },
                new InsuranceCompany { Id = 6, Name = "Mapfre Sigorta", Code = "MAPFRE", IsActive = true, DisplayOrder = 6, CreatedOn = seedDate },
                new InsuranceCompany { Id = 7, Name = "HDI Sigorta", Code = "HDI", IsActive = true, DisplayOrder = 7, CreatedOn = seedDate },
                new InsuranceCompany { Id = 8, Name = "Zurich Sigorta", Code = "ZURICH", IsActive = true, DisplayOrder = 8, CreatedOn = seedDate },
                new InsuranceCompany { Id = 9, Name = "Groupama Sigorta", Code = "GROUPAMA", IsActive = true, DisplayOrder = 9, CreatedOn = seedDate },
                new InsuranceCompany { Id = 10, Name = "Türkiye Sigorta", Code = "TURKIYE", IsActive = true, DisplayOrder = 10, CreatedOn = seedDate },
                new InsuranceCompany { Id = 11, Name = "Generali Sigorta", Code = "GENERALI", IsActive = true, DisplayOrder = 11, CreatedOn = seedDate },
                new InsuranceCompany { Id = 12, Name = "Doga Sigorta", Code = "DOGA", IsActive = true, DisplayOrder = 12, CreatedOn = seedDate },
                new InsuranceCompany { Id = 13, Name = "Neova Sigorta", Code = "NEOVA", IsActive = true, DisplayOrder = 13, CreatedOn = seedDate },
                new InsuranceCompany { Id = 14, Name = "Quick Sigorta", Code = "QUICK", IsActive = true, DisplayOrder = 14, CreatedOn = seedDate },
                new InsuranceCompany { Id = 15, Name = "Hepiyi Sigorta", Code = "HEPIYI", IsActive = true, DisplayOrder = 15, CreatedOn = seedDate },
                new InsuranceCompany { Id = 16, Name = "Magdeburger Sigorta", Code = "MAGDEBURGER", IsActive = true, DisplayOrder = 16, CreatedOn = seedDate },
                new InsuranceCompany { Id = 17, Name = "Koru Sigorta", Code = "KORU", IsActive = true, DisplayOrder = 17, CreatedOn = seedDate },
                new InsuranceCompany { Id = 18, Name = "Turk Nippon Sigorta", Code = "TURKNIPPON", IsActive = true, DisplayOrder = 18, CreatedOn = seedDate },
                new InsuranceCompany { Id = 19, Name = "Corpus Sigorta", Code = "CORPUS", IsActive = true, DisplayOrder = 19, CreatedOn = seedDate },
                new InsuranceCompany { Id = 20, Name = "Orient Sigorta", Code = "ORIENT", IsActive = true, DisplayOrder = 20, CreatedOn = seedDate }
            );

            // Seed a default tenant/agency for development
            modelBuilder.Entity<TenantEntity>().HasData(
                new TenantEntity
                {
                    Id = 1,
                    Name = "Default Insurance Agency",
                    Identifier = "default",
                    ConnectionString = "Data Source=localhost;Initial Catalog=DefaultAgencyDb;Integrated Security=True;Trust Server Certificate=True",
                    IsActive = true,
                    CreatedOn = seedDate,
                    SubscriptionPlan = "Premium",
                    MaxUsers = 50,
                    MaxStorageBytes = 5L * 1024 * 1024 * 1024, // 5GB
                    ContactEmail = "admin@default-agency.com",
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr",
                    // Agency-specific fields
                    Status = Domain.Enums.AgencyStatus.Active,
                    SubscriptionType = Domain.Enums.SubscriptionType.Premium,
                    MaxPolicies = 20000,
                    ExternalId = "A001"
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