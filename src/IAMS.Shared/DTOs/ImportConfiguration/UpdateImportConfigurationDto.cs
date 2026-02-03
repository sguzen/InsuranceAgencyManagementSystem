using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.ImportConfiguration
{
    public class UpdateImportConfigurationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int InsuranceCompanyId { get; set; }
        public ImportSourceType SourceType { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiUsername { get; set; }
        public string? ApiPassword { get; set; }
        public string? PoliciesEndpoint { get; set; }
        public string? CustomHeaders { get; set; }
        public string? AdditionalSettings { get; set; }
        public bool IsActive { get; set; }
        public bool EnableAutoSync { get; set; }
        public int? SyncIntervalMinutes { get; set; }
    }
}
