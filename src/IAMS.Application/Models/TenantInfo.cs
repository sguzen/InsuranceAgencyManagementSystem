namespace IAMS.Application.Models
{
    public class TenantInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<string> EnabledModules { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
    }
}