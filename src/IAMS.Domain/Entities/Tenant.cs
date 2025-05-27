namespace IAMS.Domain.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty; // Subdomain or unique identifier
        public string? ConnectionString { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? Settings { get; set; } // JSON settings
        public string? ModuleSettings { get; set; } // JSON for module on/off switches
    }
}