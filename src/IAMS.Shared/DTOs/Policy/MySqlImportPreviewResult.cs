namespace IAMS.Shared.DTOs.Policy
{
    /// <summary>
    /// Result of previewing MySQL import. Includes the tenant insurance company ID
    /// needed for the subsequent save-with-mapping call.
    /// </summary>
    public class MySqlImportPreviewResult
    {
        public int TenantInsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public List<PolicyImportPreviewDto> Policies { get; set; } = new();
    }
}
