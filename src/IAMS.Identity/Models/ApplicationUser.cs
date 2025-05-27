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
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
        public int TenantId { get; set; }
    }
}