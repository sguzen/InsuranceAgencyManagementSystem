using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.ImportConfiguration
{
    public class ImportConfigurationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string? InsuranceCompanyName { get; set; }
        public ImportSourceType SourceType { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiUsername { get; set; }
        public string? PoliciesEndpoint { get; set; }
        public string? CustomHeaders { get; set; }
        public string? AdditionalSettings { get; set; }
        public bool IsActive { get; set; }
        public bool EnableAutoSync { get; set; }
        public int? SyncIntervalMinutes { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string? LastSyncStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
