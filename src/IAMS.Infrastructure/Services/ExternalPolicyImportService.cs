using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Service for importing policies from external API sources
    /// </summary>
    public class ExternalPolicyImportService : IExternalPolicyImportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalPolicyImportService> _logger;

        public ExternalPolicyImportService(
            IHttpClientFactory httpClientFactory,
            ILogger<ExternalPolicyImportService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Fetch policies from external API using configuration
        /// </summary>
        public async Task<List<ImportPolicyDto>> FetchPoliciesAsync(
            ImportConfiguration configuration,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching policies from API: {ApiUrl}", configuration.ApiBaseUrl);

                using var httpClient = CreateConfiguredHttpClient(configuration);

                // Build the endpoint URL with query parameters
                var endpoint = configuration.PoliciesEndpoint ?? "/api/policies";
                var queryParams = new List<string>();

                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                if (queryParams.Any())
                    endpoint += "?" + string.Join("&", queryParams);

                _logger.LogDebug("Calling endpoint: {Endpoint}", endpoint);

                // Make the API request
                var response = await httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var policies = await response.Content.ReadFromJsonAsync<List<ImportPolicyDto>>(cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully fetched {Count} policies from API", policies?.Count ?? 0);

                return policies ?? new List<ImportPolicyDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching policies from API: {ApiUrl}", configuration.ApiBaseUrl);
                throw new InvalidOperationException($"Failed to fetch policies from API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response from: {ApiUrl}", configuration.ApiBaseUrl);
                throw new InvalidOperationException($"Failed to parse API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching policies from API: {ApiUrl}", configuration.ApiBaseUrl);
                throw;
            }
        }

        /// <summary>
        /// Test API connection with given configuration
        /// </summary>
        public async Task<bool> TestConnectionAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Testing connection to API: {ApiUrl}", configuration.ApiBaseUrl);

                using var httpClient = CreateConfiguredHttpClient(configuration);

                // Try to access the base URL or a health endpoint
                var testEndpoint = "/health"; // Most APIs have a health endpoint
                var response = await httpClient.GetAsync(testEndpoint, cancellationToken);

                var success = response.IsSuccessStatusCode;

                if (success)
                {
                    _logger.LogInformation("Successfully connected to API: {ApiUrl}", configuration.ApiBaseUrl);
                }
                else
                {
                    _logger.LogWarning("Failed to connect to API: {ApiUrl}. Status: {StatusCode}",
                        configuration.ApiBaseUrl, response.StatusCode);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection to API: {ApiUrl}", configuration.ApiBaseUrl);
                return false;
            }
        }

        /// <summary>
        /// Get available insurance companies from external API
        /// </summary>
        public async Task<List<string>> GetAvailableInsuranceCompaniesAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching available insurance companies from API: {ApiUrl}", configuration.ApiBaseUrl);

                using var httpClient = CreateConfiguredHttpClient(configuration);

                // Call the insurance companies endpoint
                var endpoint = "/api/insurance-companies";
                var response = await httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();

                // Deserialize the response
                var companies = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully fetched {Count} insurance companies from API", companies?.Count ?? 0);

                return companies ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching insurance companies from API: {ApiUrl}", configuration.ApiBaseUrl);
                throw new InvalidOperationException($"Failed to fetch insurance companies from API: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching insurance companies from API: {ApiUrl}", configuration.ApiBaseUrl);
                throw;
            }
        }

        /// <summary>
        /// Creates an HttpClient configured with the settings from ImportConfiguration
        /// </summary>
        private HttpClient CreateConfiguredHttpClient(ImportConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.ApiBaseUrl))
            {
                throw new ArgumentException("API Base URL is required", nameof(configuration.ApiBaseUrl));
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(configuration.ApiBaseUrl);

            // Set default timeout
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Configure authentication
            if (!string.IsNullOrWhiteSpace(configuration.ApiKey))
            {
                // Bearer token authentication
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", configuration.ApiKey);
            }
            else if (!string.IsNullOrWhiteSpace(configuration.ApiUsername) &&
                     !string.IsNullOrWhiteSpace(configuration.ApiPassword))
            {
                // Basic authentication
                var authBytes = System.Text.Encoding.UTF8.GetBytes($"{configuration.ApiUsername}:{configuration.ApiPassword}");
                var authHeader = Convert.ToBase64String(authBytes);
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", authHeader);
            }

            // Add custom headers if configured
            if (!string.IsNullOrWhiteSpace(configuration.CustomHeaders))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(configuration.CustomHeaders);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse custom headers JSON: {CustomHeaders}", configuration.CustomHeaders);
                }
            }

            return httpClient;
        }
    }
}
