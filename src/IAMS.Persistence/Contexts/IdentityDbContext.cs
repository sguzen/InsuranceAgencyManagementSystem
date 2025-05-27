using IAMS.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Identity.Contexts
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Permission entity
            builder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Module).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.Name, e.Module }).IsUnique();
            });

            // Configure RolePermission entity
            builder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany()
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default permissions
            SeedPermissions(builder);
        }

        private static void SeedPermissions(ModelBuilder builder)
        {
            var permissions = new List<Permission>
            {
                // User Management
                new() { Id = 1, Name = "Users.View", Description = "View users", Module = "UserManagement" },
                new() { Id = 2, Name = "Users.Create", Description = "Create users", Module = "UserManagement" },
                new() { Id = 3, Name = "Users.Edit", Description = "Edit users", Module = "UserManagement" },
                new() { Id = 4, Name = "Users.Delete", Description = "Delete users", Module = "UserManagement" },

                // Role Management
                new() { Id = 5, Name = "Roles.View", Description = "View roles", Module = "UserManagement" },
                new() { Id = 6, Name = "Roles.Create", Description = "Create roles", Module = "UserManagement" },
                new() { Id = 7, Name = "Roles.Edit", Description = "Edit roles", Module = "UserManagement" },
                new() { Id = 8, Name = "Roles.Delete", Description = "Delete roles", Module = "UserManagement" },

                // Customer Management
                new() { Id = 9, Name = "Customers.View", Description = "View customers", Module = "CustomerManagement" },
                new() { Id = 10, Name = "Customers.Create", Description = "Create customers", Module = "CustomerManagement" },
                new() { Id = 11, Name = "Customers.Edit", Description = "Edit customers", Module = "CustomerManagement" },
                new() { Id = 12, Name = "Customers.Delete", Description = "Delete customers", Module = "CustomerManagement" },

                // Policy Management
                new() { Id = 13, Name = "Policies.View", Description = "View policies", Module = "PolicyManagement" },
                new() { Id = 14, Name = "Policies.Create", Description = "Create policies", Module = "PolicyManagement" },
                new() { Id = 15, Name = "Policies.Edit", Description = "Edit policies", Module = "PolicyManagement" },
                new() { Id = 16, Name = "Policies.Delete", Description = "Delete policies", Module = "PolicyManagement" },

                // Reports
                new() { Id = 17, Name = "Reports.View", Description = "View reports", Module = "Reports" },
                new() { Id = 18, Name = "Reports.Export", Description = "Export reports", Module = "Reports" },

                // Accounting
                new() { Id = 19, Name = "Accounting.View", Description = "View accounting", Module = "Accounting" },
                new() { Id = 20, Name = "Accounting.Manage", Description = "Manage accounting", Module = "Accounting" },

                // System Settings
                new() { Id = 21, Name = "Settings.View", Description = "View settings", Module = "System" },
                new() { Id = 22, Name = "Settings.Manage", Description = "Manage settings", Module = "System" }
            };

            builder.Entity<Permission>().HasData(permissions);
        }
    }

    // IAMS.Identity/Models/Permission.cs
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
    }

    // IAMS.Identity/Models/RolePermission.cs
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }

        public virtual ApplicationRole Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}