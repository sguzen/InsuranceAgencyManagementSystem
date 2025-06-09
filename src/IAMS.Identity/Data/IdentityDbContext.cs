// IAMS.Identity/Data/IdentityDbContext.cs
using IAMS.Identity.Models;
using IAMS.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Identity.Data
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public IdentityDbContext(
            DbContextOptions<IdentityDbContext> options,
            ITenantContextAccessor tenantContextAccessor = null) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure tenant filtering for ApplicationUser
            if (_tenantContextAccessor?.TenantContext?.Tenant != null)
            {
                builder.Entity<ApplicationUser>()
                    .HasQueryFilter(u => u.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);

                builder.Entity<ApplicationRole>()
                    .HasQueryFilter(r => r.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);
            }

            // Identity table renaming
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Module).HasMaxLength(100);
                entity.HasIndex(p => p.Name).IsUnique();
            });

            // Configure RolePermission entity (many-to-many)
            builder.Entity<RolePermission>(entity =>
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
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.HasIndex(u => new { u.Email, u.TenantId }).IsUnique();
            });

            // Additional configurations for ApplicationRole
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(r => r.Description).HasMaxLength(500);
                entity.HasIndex(r => new { r.Name, r.TenantId }).IsUnique();
            });
        }
    }
}