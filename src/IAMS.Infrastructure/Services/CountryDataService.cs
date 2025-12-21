using IAMS.Application.Interfaces;
using IAMS.Shared.DTOs.Parametric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Service for integrating with external country data API
    /// Base URL: https://test.websuresoft.com/das
    /// Endpoint: /dasapi/UlkKod/GetAll
    /// </summary>
    public class CountryDataService : ICountryDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CountryDataService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public CountryDataService(
            IHttpClientFactory httpClientFactory,
            ILogger<CountryDataService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;

            // Use the same configuration as VehicleDataApi for consistency
            _baseUrl = _configuration["VehicleDataApi:BaseUrl"] ?? "https://test.websuresoft.com/das";
            _timeoutSeconds = int.Parse(_configuration["VehicleDataApi:TimeoutSeconds"] ?? "30");

            _logger.LogInformation("CountryDataService initialized with BaseUrl: {BaseUrl}", _baseUrl);
        }

        public async Task<List<ExternalCountryDataDto>> FetchCountryDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting country data fetch from external API");

                using var httpClient = CreateHttpClient();
                var endpoint = "/dasapi/UlkKod/GetAll";
                var requestUrl = $"{_baseUrl}{endpoint}";

                _logger.LogDebug("Fetching country data from: {RequestUrl}", requestUrl);

                var response = await httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Country data API returned error status {StatusCode}. Response: {ErrorContent}",
                        response.StatusCode,
                        errorContent);
                    throw new HttpRequestException(
                        $"Country data API request failed with status {response.StatusCode}: {errorContent}");
                }

                var countryData = await response.Content.ReadFromJsonAsync<List<ExternalCountryDataDto>>();

                if (countryData == null)
                {
                    _logger.LogWarning("Country data API returned null response");
                    return new List<ExternalCountryDataDto>();
                }

                _logger.LogInformation(
                    "Successfully fetched {Count} country records from external API",
                    countryData.Count);

                return countryData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching country data from external API");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error while processing country data response");
                throw new InvalidOperationException("Failed to parse country data response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching country data from external API");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing connection to country data API");

                using var httpClient = CreateHttpClient();
                var endpoint = "/dasapi/UlkKod/GetAll";
                var requestUrl = $"{_baseUrl}{endpoint}";

                var response = await httpClient.GetAsync(requestUrl);

                var isSuccessful = response.IsSuccessStatusCode;

                if (isSuccessful)
                {
                    _logger.LogInformation("Country data API connection test successful");
                }
                else
                {
                    _logger.LogWarning(
                        "Country data API connection test failed with status {StatusCode}",
                        response.StatusCode);
                }

                return isSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Country data API connection test failed with exception");
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
