using System.Text.Json.Serialization;

namespace IAMS.Application.DTOs.Parametric
{
    /// <summary>
    /// DTO for country data from external DAS API
    /// Maps to response from /dasapi/UlkKod/GetAll
    /// </summary>
    public class ExternalCountryDataDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("kod")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("ad")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("adEn")]
        public string? NameEn { get; set; }

        [JsonPropertyName("telefonKodu")]
        public string? PhoneCode { get; set; }

        [JsonPropertyName("aktif")]
        public bool? IsActive { get; set; }

        [JsonPropertyName("sira")]
        public int? DisplayOrder { get; set; }
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
