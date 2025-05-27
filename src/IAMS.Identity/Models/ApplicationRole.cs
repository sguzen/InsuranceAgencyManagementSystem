using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Models
{
    public class ApplicationRole : IdentityRole
    {
        public int TenantId { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; } // System roles cannot be modified
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<RolePermission> Permissions { get; set; }
    }

    // IAMS.Identity/Models/Permission.cs
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Module { get; set; } // Optional: associated module
        public bool IsSystem { get; set; } // System permissions cannot be modified

        // Navigation property
        public ICollection<RolePermission> RolePermissions { get; set; }
    }

    // IAMS.Identity/Models/RolePermission.cs
    public class RolePermission
    {
        public string RoleId { get; set; }
        public ApplicationRole Role { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}
