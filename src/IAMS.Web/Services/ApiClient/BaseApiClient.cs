using IAMS.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace IAMS.Web.Services.ApiClient
{
    /// <summary>
    /// Base API client for making HTTP requests to the IAMS API
    /// Handles authentication, error handling, and serialization
    /// </summary>
    public class BaseApiClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly JsonSerializerOptions _jsonOptions;

        public BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected async Task<Result<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<T>.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                return result ?? Result<T>.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<T>.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                return result ?? Result<T>.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result> PostAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<T>.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<T>>(_jsonOptions);
                return result ?? Result<T>.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result> PutAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result> PatchAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync(endpoint, data, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }

        protected async Task<Result> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    // Try to deserialize error response as Result object
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                        if (errorResult != null)
                        {
                            return errorResult;
                        }
                    }
                    catch
                    {
                        // If deserialization fails, fall back to raw content
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API isteği başarısız: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("API'den boş yanıt alındı", new List<string>());
            }
            catch (Exception ex)
            {
                return Result.Failure($"API çağrısı başarısız: {ex.Message}", ex.ToString());
            }
        }
    }
}
