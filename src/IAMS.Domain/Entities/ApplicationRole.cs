using Microsoft.AspNetCore.Identity;

namespace IAMS.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; } // System roles cannot be modified
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}