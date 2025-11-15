using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Service for integrating with external vehicle brand/model API
    /// Base URL: https://test.websuresoft.com/das
    /// Endpoint: /dasapi/AracMarkaModel/GetAllBrandAndModels
    /// </summary>
    public class VehicleDataService : IVehicleDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VehicleDataService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public VehicleDataService(
            IHttpClientFactory httpClientFactory,
            ILogger<VehicleDataService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;

            // Read configuration with fallback to default values
            _baseUrl = _configuration["VehicleDataApi:BaseUrl"] ?? "https://test.websuresoft.com/das";
            _timeoutSeconds = int.Parse(_configuration["VehicleDataApi:TimeoutSeconds"] ?? "30");

            _logger.LogInformation("VehicleDataService initialized with BaseUrl: {BaseUrl}", _baseUrl);
        }

        public async Task<List<ExternalVehicleDataDto>> FetchVehicleDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting vehicle data fetch from external API");

                using var httpClient = CreateHttpClient();
                var endpoint = "/dasapi/AracMarkaModel/GetAllBrandAndModels";
                var requestUrl = $"{_baseUrl}{endpoint}";

                _logger.LogDebug("Fetching vehicle data from: {RequestUrl}", requestUrl);

                var response = await httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Vehicle data API returned error status {StatusCode}. Response: {ErrorContent}",
                        response.StatusCode,
                        errorContent);
                    throw new HttpRequestException(
                        $"Vehicle data API request failed with status {response.StatusCode}: {errorContent}");
                }

                var vehicleData = await response.Content.ReadFromJsonAsync<List<ExternalVehicleDataDto>>();

                if (vehicleData == null)
                {
                    _logger.LogWarning("Vehicle data API returned null response");
                    return new List<ExternalVehicleDataDto>();
                }

                _logger.LogInformation(
                    "Successfully fetched {Count} vehicle records from external API",
                    vehicleData.Count);

                return vehicleData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching vehicle data from external API");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error while processing vehicle data response");
                throw new InvalidOperationException("Failed to parse vehicle data response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching vehicle data from external API");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing connection to vehicle data API");

                using var httpClient = CreateHttpClient();
                var endpoint = "/dasapi/AracMarkaModel/GetAllBrandAndModels";
                var requestUrl = $"{_baseUrl}{endpoint}";

                // Use HEAD request if supported, otherwise GET with short timeout
                var response = await httpClient.GetAsync(requestUrl);

                var isSuccessful = response.IsSuccessStatusCode;

                if (isSuccessful)
                {
                    _logger.LogInformation("Vehicle data API connection test successful");
                }
                else
                {
                    _logger.LogWarning(
                        "Vehicle data API connection test failed with status {StatusCode}",
                        response.StatusCode);
                }

                return isSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vehicle data API connection test failed with exception");
                return false;
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

            // Add any required headers here
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // If API requires authentication, add it here:
            // var apiKey = _configuration["VehicleDataApi:ApiKey"];
            // if (!string.IsNullOrEmpty(apiKey))
            // {
            //     httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
            // }

            return httpClient;
        }
    }
}
