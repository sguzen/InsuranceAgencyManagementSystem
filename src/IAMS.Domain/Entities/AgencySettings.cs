
namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Agency-specific configuration settings stored in each agency's own database.
    /// Each agency database has its own Settings table for isolation.
    /// </summary>
    public class AgencySettings : BaseEntity
    {
        /// <summary>
        /// Setting key (e.g., "general", "smtp", "regional")
        /// </summary>
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>
        /// Setting value stored as JSON for complex objects
        /// </summary>
        public string SettingValue { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of what this setting controls
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Setting category for grouping (e.g., "General", "Email", "Integration")
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// Whether this setting is system-managed or user-configurable
        /// </summary>
        public bool IsSystemSetting { get; set; } = false;
    }
}
