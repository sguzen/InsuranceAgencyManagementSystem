using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.ImportConfiguration
{
    public class CreateImportConfigurationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int InsuranceCompanyId { get; set; }
        public ImportSourceType SourceType { get; set; }

        /// <summary>
        /// For DatabaseImport: MySQL Host (e.g., "77.92.6.20")
        /// For API: API base URL
        /// </summary>
        public string? ApiBaseUrl { get; set; }

        /// <summary>
        /// For DatabaseImport: Agency Code (e.g., "A022")
        /// For API: API key/token
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// For DatabaseImport: MySQL Username
        /// For API: API username
        /// </summary>
        public string? ApiUsername { get; set; }

        /// <summary>
        /// For DatabaseImport: MySQL Password
        /// For API: API password
        /// </summary>
        public string? ApiPassword { get; set; }

        /// <summary>
        /// For DatabaseImport: MySQL Database name (e.g., "birim2025")
        /// For API: Policies endpoint path
        /// </summary>
        public string? PoliciesEndpoint { get; set; }

        public string? CustomHeaders { get; set; }

        /// <summary>
        /// For DatabaseImport: JSON with Port, UseSsl, ConnectionTimeoutSeconds, CommandTimeoutSeconds
        /// </summary>
        public string? AdditionalSettings { get; set; }

        public bool IsActive { get; set; } = true;
        public bool EnableAutoSync { get; set; } = false;
        public int? SyncIntervalMinutes { get; set; }
    }
}
