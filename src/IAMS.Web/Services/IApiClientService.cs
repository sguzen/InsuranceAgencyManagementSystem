using System.Net.Http.Headers;
using System.Text.Json;

namespace IAMS.Web.Services
{
    public interface IApiClientService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task<bool> DeleteAsync(string endpoint);
    }

    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiClientService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClientService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiClientService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            // Configure HttpClient base address
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7192";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);

            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GET request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making GET request to {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("POST request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making POST request to {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.PutAsJsonAsync(endpoint, request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PUT request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making PUT request to {Endpoint}", endpoint);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making DELETE request to {Endpoint}", endpoint);
                return false;
            }
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var jwtToken = httpContext?.User?.FindFirst("jwt_token")?.Value;

            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
    }

    // Data transfer objects for API communication
    public class DashboardStatsDto
    {
        public int TotalCustomers { get; set; }
        public int ActivePolicies { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int ClaimsThisMonth { get; set; }
    }

}