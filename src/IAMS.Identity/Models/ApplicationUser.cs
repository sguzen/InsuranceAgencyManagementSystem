// Updated ApplicationUser.cs and ApplicationRole.cs in single file
using Microsoft.AspNetCore.Identity;

namespace IAMS.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public int TenantId { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public int TenantId { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; } // System roles cannot be modified
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }

    // Permission.cs
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Module { get; set; } // Optional: associated module
        public bool IsSystem { get; set; } // System permissions cannot be modified

        // Navigation property
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    // RolePermission.cs
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public virtual ApplicationRole Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;
    }
}