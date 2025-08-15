namespace IAMS.Domain.Entities
{
    // RolePermission.cs
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public virtual ApplicationRole Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;
    }
}