using IAMS.Application.Configuration;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// HTTP client-based implementation for external premium calculation service
    /// </summary>
    public class ExternalPremiumCalculationService : IExternalPremiumCalculationService
    {
        private readonly HttpClient _httpClient;
        private readonly PremiumCalculationSettings _settings;
        private readonly ILogger<ExternalPremiumCalculationService> _logger;

        public ExternalPremiumCalculationService(
            HttpClient httpClient,
            IOptions<PremiumCalculationSettings> settings,
            ILogger<ExternalPremiumCalculationService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Configure HTTP client
            if (!string.IsNullOrEmpty(_settings.ExternalService.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.ExternalService.BaseUrl);
            }
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.ExternalService.TimeoutSeconds);

            // Add API key header if configured
            if (!string.IsNullOrEmpty(_settings.ExternalService.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ExternalService.ApiKey);
            }
        }

        public async Task<decimal> CalculatePremiumAsync(Policy policy, string policyTypeCode)
        {
            if (!_settings.ExternalService.Enabled)
            {
                _logger.LogWarning("External calculation service is disabled");
                throw new InvalidOperationException("External calculation service is not enabled");
            }

            try
            {
                var request = new PremiumCalculationRequest
                {
                    PolicyTypeCode = policyTypeCode,
                    PolicyId = policy.Id,
                    PolicyNumber = policy.PolicyNumber,
                    PremiumAmount = policy.PremiumAmount,
                    DeductibleAmount = policy.DeductibleAmount,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    NoClaimDiscountRate = policy.NoClaimDiscountRate,
                    FleetDiscountRate = policy.FleetDiscountRate,
                    DriverAccidentCoverageAmount = policy.DriverAccidentCoverageAmount,
                    // Vehicle-specific data
                    VehicleData = policy.VehicleId.HasValue ? new VehicleCalculationData
                    {
                        VehicleId = policy.VehicleId.Value,
                        // Note: Vehicle details would need to be loaded separately if required
                    } : null,
                    // Kasko-specific coverages
                    HasGlassCoverage = policy.HasGlassCoverage,
                    HasTheftCoverage = policy.HasTheftCoverage,
                    HasNaturalDisasterCoverage = policy.HasNaturalDisasterCoverage,
                    HasDriverAccidentCoverage = policy.HasDriverAccidentCoverage
                };

                _logger.LogInformation(
                    "Sending premium calculation request to external service for policy type '{PolicyTypeCode}'",
                    policyTypeCode);

                var response = await _httpClient.PostAsJsonAsync(
                    _settings.ExternalService.CalculationEndpoint,
                    request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PremiumCalculationResponse>();

                if (result == null || !result.Success)
                {
                    var errorMessage = result?.ErrorMessage ?? "Unknown error from external service";
                    _logger.LogError("External calculation service returned error: {ErrorMessage}", errorMessage);
                    throw new InvalidOperationException($"External calculation failed: {errorMessage}");
                }

                _logger.LogInformation(
                    "Successfully calculated premium using external service. Amount: {Premium}",
                    result.PremiumAmount);

                return result.PremiumAmount;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while calling external calculation service");
                throw new InvalidOperationException("Failed to communicate with external calculation service", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to external calculation service timed out");
                throw new InvalidOperationException("External calculation service request timed out", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from external calculation service");
                throw new InvalidOperationException("Invalid response from external calculation service", ex);
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            if (!_settings.ExternalService.Enabled)
            {
                return false;
            }

            try
            {
                var response = await _httpClient.GetAsync(_settings.ExternalService.HealthCheckEndpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "External calculation service health check failed");
                return false;
            }
        }
    }

    #region Request/Response DTOs

    /// <summary>
    /// Request model for external premium calculation
    /// </summary>
    public class PremiumCalculationRequest
    {
        public string PolicyTypeCode { get; set; } = string.Empty;
        public int PolicyId { get; set; }
        public string? PolicyNumber { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal? DeductibleAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? NoClaimDiscountRate { get; set; }
        public decimal? FleetDiscountRate { get; set; }
        public int? DriverAccidentCoverageAmount { get; set; }
        public VehicleCalculationData? VehicleData { get; set; }
        public bool? HasGlassCoverage { get; set; }
        public bool? HasTheftCoverage { get; set; }
        public bool? HasNaturalDisasterCoverage { get; set; }
        public bool? HasDriverAccidentCoverage { get; set; }
    }

    /// <summary>
    /// Vehicle data for calculation
    /// </summary>
    public class VehicleCalculationData
    {
        public int VehicleId { get; set; }
        public string? PlateNumber { get; set; }
        public int? ModelYear { get; set; }
        public decimal? CurrentValue { get; set; }
        public int? EnginePower { get; set; }
    }

    /// <summary>
    /// Response model from external premium calculation
    /// </summary>
    public class PremiumCalculationResponse
    {
        public bool Success { get; set; }
        public decimal PremiumAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, decimal>? BreakDown { get; set; }
    }

    #endregion
}
