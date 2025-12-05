using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Configuration for external API integrations and import sources
    /// </summary>
    public class ImportConfiguration : BaseEntity
    {
        /// <summary>
        /// Configuration name/identifier
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of this import configuration
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Insurance Company this configuration is for
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;

        /// <summary>
        /// Source type for this configuration
        /// </summary>
        public ImportSourceType SourceType { get; set; }

        /// <summary>
        /// API base URL (for API integrations)
        /// </summary>
        public string? ApiBaseUrl { get; set; }

        /// <summary>
        /// API key or token (encrypted)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// API username (if using basic auth)
        /// </summary>
        public string? ApiUsername { get; set; }

        /// <summary>
        /// API password (encrypted, if using basic auth)
        /// </summary>
        public string? ApiPassword { get; set; }

        /// <summary>
        /// Additional headers required for API calls (JSON format)
        /// </summary>
        public string? CustomHeaders { get; set; }

        /// <summary>
        /// API endpoint path for fetching policies
        /// </summary>
        public string? PoliciesEndpoint { get; set; }

        /// <summary>
        /// Whether this configuration is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether to auto-sync on a schedule
        /// </summary>
        public bool EnableAutoSync { get; set; } = false;

        /// <summary>
        /// Sync interval in minutes (for auto-sync)
        /// </summary>
        public int? SyncIntervalMinutes { get; set; }

        /// <summary>
        /// Last successful sync date
        /// </summary>
        public DateTime? LastSyncDate { get; set; }

        /// <summary>
        /// Last sync status
        /// </summary>
        public string? LastSyncStatus { get; set; }

        /// <summary>
        /// Additional configuration settings (JSON format)
        /// </summary>
        public string? AdditionalSettings { get; set; }

        // Navigation properties
        public virtual ICollection<ImportHistory> ImportHistories { get; set; } = new List<ImportHistory>();
    }
}
