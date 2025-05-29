// Fixed IdentityDbContext.cs
using IAMS.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Identity.Data
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationUser, string>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly string? _connectionString;

        public IdentityDbContext(
            DbContextOptions<IdentityDbContext> options,
            ITenantContextAccessor tenantContextAccessor) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;

            // Get tenant-specific connection string
            if (_tenantContextAccessor?.TenantContext?.Tenant != null)
            {
                _connectionString = _tenantContextAccessor.TenantContext.Tenant.ConnectionString;
            }
        }

        // DbSets for additional entities
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure tenant filtering for ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => _tenantContextAccessor.TenantContext == null ||
                                   u.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);

            // Configure tenant filtering for ApplicationRole
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(r => _tenantContextAccessor.TenantContext == null ||
                                   r.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);

            // Identity table renaming
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationUser>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Module).HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure RolePermission entity (many-to-many)
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Permissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationRole
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TenantId).IsRequired();
                entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
            });

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TenantId).IsRequired();
                entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            });
        }
    }

    // Create an alias for the context used in seeder to avoid confusion
    public class AppIdentityDbContext : IdentityDbContext
    {
        public AppIdentityDbContext(
            DbContextOptions<IdentityDbContext> options,
            ITenantContextAccessor tenantContextAccessor) : base(options, tenantContextAccessor)
        {
        }
    }
}