using System.Text.Json.Serialization;

namespace IAMS.Shared.DTOs.Parametric
{
    /// <summary>
    /// DTO for country data from external DAS API
    /// Maps to response from /dasapi/UlkKod/GetAll
    /// </summary>
    public class ExternalCountryDataDto
    {

        [JsonPropertyName("kod")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("adi")]
        public string Name { get; set; } = string.Empty;
       
    }

    /// <summary>
    /// Result of country data sync operation
    /// </summary>
    public class CountryDataSyncResult
    {
        public bool Success { get; set; }
        public int CountriesCreated { get; set; }
        public int CountriesUpdated { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
