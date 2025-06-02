// IAMS.Infrastructure/Services/IntegrationService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Data;
using System.Net.Http.Json;

namespace IAMS.Infrastructure.Services
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IntegrationService> _logger;
        private readonly IntegrationDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;

        public IntegrationService(
            IConfiguration configuration,
            ILogger<IntegrationService> logger,
            IntegrationDbContext dbContext,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IntegrationResult> SyncCustomerDataAsync(int customerId)
        {
            var result = new IntegrationResult();

            try
            {
                _logger.LogInformation("Starting customer data sync for customer {CustomerId}", customerId);

                // Get enabled providers
                var providers = await GetEnabledProvidersAsync();
                var syncResults = new Dictionary<string, object>();

                foreach (var provider in providers)
                {
                    try
                    {
                        var providerResult = await SyncCustomerWithProviderAsync(customerId, provider);
                        syncResults[provider.Name] = providerResult;

                        await LogIntegrationAsync(provider.Name, "customer_sync", true, customerId.ToString(), JsonSerializer.Serialize(providerResult));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync customer {CustomerId} with provider {Provider}", customerId, provider.Name);
                        syncResults[provider.Name] = new { error = ex.Message };

                        await LogIntegrationAsync(provider.Name, "customer_sync", false, customerId.ToString(), null, ex.Message);
                    }
                }

                result.Success = syncResults.Values.All(r => !r.GetType().GetProperty("error")?.GetValue(r)?.ToString()?.Any() == true);
                result.Data = syncResults;

                _logger.LogInformation("Customer data sync completed for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during customer data sync for customer {CustomerId}", customerId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<IntegrationResult> SyncPolicyDataAsync(int policyId)
        {
            var result = new IntegrationResult();

            try
            {
                _logger.LogInformation("Starting policy data sync for policy {PolicyId}", policyId);

                var providers = await GetEnabledProvidersAsync();
                var syncResults = new Dictionary<string, object>();

                foreach (var provider in providers)
                {
                    try
                    {
                        var providerResult = await SyncPolicyWithProviderAsync(policyId, provider);
                        syncResults[provider.Name] = providerResult;

                        await LogIntegrationAsync(provider.Name, "policy_sync", true, policyId.ToString(), JsonSerializer.Serialize(providerResult));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync policy {PolicyId} with provider {Provider}", policyId, provider.Name);
                        syncResults[provider.Name] = new { error = ex.Message };

                        await LogIntegrationAsync(provider.Name, "policy_sync", false, policyId.ToString(), null, ex.Message);
                    }
                }

                result.Success = syncResults.Values.All(r => !r.GetType().GetProperty("error")?.GetValue(r)?.ToString()?.Any() == true);
                result.Data = syncResults;

                _logger.LogInformation("Policy data sync completed for policy {PolicyId}", policyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during policy data sync for policy {PolicyId}", policyId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<IntegrationResult> SubmitClaimAsync(int claimId)
        {
            var result = new IntegrationResult();

            try
            {
                _logger.LogInformation("Starting claim submission for claim {ClaimId}", claimId);

                // Get the appropriate provider for the claim (based on insurance company)
                var provider = await GetClaimProviderAsync(claimId);

                if (provider == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "No provider found for this claim";
                    return result;
                }

                var claimResult = await SubmitClaimToProviderAsync(claimId, provider);
                result.Success = true;
                result.Data["claimSubmission"] = claimResult;

                await LogIntegrationAsync(provider.Name, "claim_submission", true, claimId.ToString(), JsonSerializer.Serialize(claimResult));

                _logger.LogInformation("Claim submission completed for claim {ClaimId}", claimId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during claim submission for claim {ClaimId}", claimId);
                result.Success = false;
                result.ErrorMessage = ex.Message;

                await LogIntegrationAsync("unknown", "claim_submission", false, claimId.ToString(), null, ex.Message);
            }

            return result;
        }

        public async Task<List<IntegrationLog>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.IntegrationLogs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.CreatedAt <= toDate.Value);

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<bool> TestConnectionAsync(string providerName)
        {
            try
            {
                var provider = await GetProviderConfigAsync(providerName);
                if (provider == null)
                {
                    return false;
                }

                using var httpClient = _httpClientFactory.CreateClient();

                // Configure client based on provider settings
                ConfigureHttpClient(httpClient, provider);

                // Make a test request to the provider's health endpoint
                var testEndpoint = provider.Settings.GetValueOrDefault("TestEndpoint", "/health");
                var response = await httpClient.GetAsync(testEndpoint);

                var success = response.IsSuccessStatusCode;

                await LogIntegrationAsync(providerName, "connection_test", success, null,
                    success ? "Connection successful" : $"HTTP {(int)response.StatusCode}",
                    success ? null : $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed for provider {Provider}", providerName);
                await LogIntegrationAsync(providerName, "connection_test", false, null, null, ex.Message);
                return false;
            }
        }

        public async Task<List<IntegrationProvider>> GetAvailableProvidersAsync()
        {
            // Get configured providers from database and configuration
            var configuredProviders = _configuration.GetSection("IntegrationProviders").Get<List<IntegrationProviderConfig>>() ?? new();

            var providers = new List<IntegrationProvider>();

            foreach (var config in configuredProviders)
            {
                var provider = new IntegrationProvider
                {
                    Name = config.Name,
                    DisplayName = config.DisplayName,
                    IsEnabled = config.IsEnabled,
                    Settings = config.Settings,
                    IsConnected = await TestConnectionAsync(config.Name)
                };

                // Get last sync time from logs
                var lastSync = await _dbContext.IntegrationLogs
                    .Where(l => l.Provider == config.Name && l.Success)
                    .OrderByDescending(l => l.CreatedAt)
                    .Select(l => l.CreatedAt)
                    .FirstOrDefaultAsync();

                provider.LastSync = lastSync == default ? null : lastSync;
                providers.Add(provider);
            }

            return providers;
        }

        private async Task<List<IntegrationProvider>> GetEnabledProvidersAsync()
        {
            var allProviders = await GetAvailableProvidersAsync();
            return allProviders.Where(p => p.IsEnabled).ToList();
        }

        private async Task<IntegrationProvider?> GetProviderConfigAsync(string providerName)
        {
            var providers = await GetAvailableProvidersAsync();
            return providers.FirstOrDefault(p => p.Name == providerName);
        }

        private async Task<IntegrationProvider?> GetClaimProviderAsync(int claimId)
        {
            // In a real implementation, you would determine the provider based on the claim's insurance company
            // For now, return the first enabled provider
            var providers = await GetEnabledProvidersAsync();
            return providers.FirstOrDefault();
        }

        private async Task<object> SyncCustomerWithProviderAsync(int customerId, IntegrationProvider provider)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            ConfigureHttpClient(httpClient, provider);

            // Get customer data (this would come from your customer service)
            var customerData = new
            {
                id = customerId,
                // Add customer properties
            };

            var endpoint = provider.Settings.GetValueOrDefault("CustomerSyncEndpoint", "/api/customers/sync");
            var response = await httpClient.PostAsJsonAsync(endpoint, customerData);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(responseContent) ?? new();
        }

        private async Task<object> SyncPolicyWithProviderAsync(int policyId, IntegrationProvider provider)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            ConfigureHttpClient(httpClient, provider);

            // Get policy data (this would come from your policy service)
            var policyData = new
            {
                id = policyId,
                // Add policy properties
            };

            var endpoint = provider.Settings.GetValueOrDefault("PolicySyncEndpoint", "/api/policies/sync");
            var response = await httpClient.PostAsJsonAsync(endpoint, policyData);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(responseContent) ?? new();
        }

        private async Task<object> SubmitClaimToProviderAsync(int claimId, IntegrationProvider provider)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            ConfigureHttpClient(httpClient, provider);

            // Get claim data (this would come from your claim service)
            var claimData = new
            {
                id = claimId,
                // Add claim properties
            };

            var endpoint = provider.Settings.GetValueOrDefault("ClaimSubmissionEndpoint", "/api/claims/submit");
            var response = await httpClient.PostAsJsonAsync(endpoint, claimData);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(responseContent) ?? new();
        }

        private void ConfigureHttpClient(HttpClient httpClient, IntegrationProvider provider)
        {
            // Set base URL
            if (provider.Settings.TryGetValue("BaseUrl", out var baseUrl))
            {
                httpClient.BaseAddress = new Uri(baseUrl);
            }

            // Set authentication
            if (provider.Settings.TryGetValue("AuthType", out var authType))
            {
                switch (authType.ToLower())
                {
                    case "bearer":
                        if (provider.Settings.TryGetValue("Token", out var token))
                        {
                            httpClient.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        }
                        break;
                    case "apikey":
                        if (provider.Settings.TryGetValue("ApiKey", out var apiKey) &&
                            provider.Settings.TryGetValue("ApiKeyHeader", out var header))
                        {
                            httpClient.DefaultRequestHeaders.Add(header, apiKey);
                        }
                        break;
                }
            }

            // Set timeout
            if (provider.Settings.TryGetValue("TimeoutSeconds", out var timeoutStr) &&
                int.TryParse(timeoutStr, out var timeout))
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            }
        }

        private async Task LogIntegrationAsync(string provider, string operation, bool success,
            string? requestData = null, string? responseData = null, string? errorMessage = null)
        {
            var log = new IntegrationLogEntity
            {
                Provider = provider,
                Operation = operation,
                Success = success,
                RequestData = requestData,
                ResponseData = responseData,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.IntegrationLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
    }

    public class IntegrationProviderConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public Dictionary<string, string> Settings { get; set; } = new();
    }
}