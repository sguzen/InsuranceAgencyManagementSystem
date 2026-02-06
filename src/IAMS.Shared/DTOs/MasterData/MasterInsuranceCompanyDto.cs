namespace IAMS.Shared.DTOs.MasterData
{
    /// <summary>
    /// DTO for insurance company in the master database (system-wide list)
    /// </summary>
    public class MasterInsuranceCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? IntegrationSettings { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Aggregated info
        public int LinkedAgenciesCount { get; set; }
    }

    public class CreateMasterInsuranceCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? IntegrationSettings { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateMasterInsuranceCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? IntegrationSettings { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Simple DTO for dropdowns
    /// </summary>
    public class InsuranceCompanySelectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
