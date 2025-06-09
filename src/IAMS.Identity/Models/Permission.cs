// Updated Permission.cs and ApplicationRole.cs in single file
namespace IAMS.Identity.Models
{
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
}