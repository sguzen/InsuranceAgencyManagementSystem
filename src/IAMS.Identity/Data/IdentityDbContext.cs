using IAMS.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Data
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly string _connectionString;

        public AppIdentityDbContext(
            DbContextOptions<AppIdentityDbContext> options,
            ITenantContextAccessor tenantContextAccessor) : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;

            // Get tenant-specific connection string
            if (_tenantContextAccessor?.TenantContext?.Tenant != null)
            {
                _connectionString = _tenantContextAccessor.TenantContext.Tenant.ConnectionString;
            }
        }

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
                .HasQueryFilter(u => u.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);

            // Configure tenant filtering for ApplicationRole
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => r.TenantId == _tenantContextAccessor.TenantContext.Tenant.Id);

            // Identity table renaming
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}
