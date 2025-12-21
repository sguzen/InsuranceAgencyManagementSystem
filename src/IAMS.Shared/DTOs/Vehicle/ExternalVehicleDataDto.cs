using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IAMS.Shared.DTOs.Vehicle
{
    /// <summary>
    /// DTO for vehicle brand and model data from external API
    /// Maps to response from /dasapi/AracMarkaModel/GetAllBrandAndModels
    /// </summary>
    public class ExternalVehicleDataDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("brandModelName")]
        public string BrandModelName { get; set; } = string.Empty;

        [JsonPropertyName("brandCode")]
        public int BrandCode { get; set; }

        [JsonPropertyName("brandName")]
        public string BrandName { get; set; } = string.Empty;

        [JsonPropertyName("modelName")]
        public string ModelName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of vehicle data sync operation
    /// </summary>
    public class VehicleDataSyncResult
    {
        public bool Success { get; set; }
        public int BrandsCreated { get; set; }
        public int BrandsUpdated { get; set; }
        public int ModelsCreated { get; set; }
        public int ModelsUpdated { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
