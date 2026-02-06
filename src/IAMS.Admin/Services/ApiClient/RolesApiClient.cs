using IAMS.Shared.DTOs.Identity;
using IAMS.Shared.Models;
using System.Net.Http.Json;

namespace IAMS.Admin.Services.ApiClient
{
    public interface IRolesApiClient
    {
        Task<Result<List<RoleDto>>> GetRolesAsync(int page = 1, int pageSize = 50, string? search = null);
        Task<Result<RoleDto>> GetRoleByIdAsync(string roleId);
        Task<Result<string>> CreateRoleAsync(CreateRoleRequest request);
        Task<Result> UpdateRoleAsync(string roleId, UpdateRoleRequest request);
        Task<Result> DeleteRoleAsync(string roleId);
        Task<Result<List<PermissionDto>>> GetRolePermissionsAsync(string roleId);
        Task<Result> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds);
        Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(string? module = null);
    }

    public class RolesApiClient : BaseApiClient, IRolesApiClient
    {
        public RolesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<RoleDto>>> GetRolesAsync(int page = 1, int pageSize = 50, string? search = null)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
                queryString += $"&search={Uri.EscapeDataString(search)}";

            try
            {
                var response = await _httpClient.GetAsync($"api/roles{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<RoleDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<RolesResponse>(_jsonOptions);
                if (result != null)
                {
                    return Result<List<RoleDto>>.Success(result.Roles, "Roles loaded");
                }

                return Result<List<RoleDto>>.Failure("Empty response", (List<string>?)null);
            }
            catch (Exception ex)
            {
                return Result<List<RoleDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<RoleDto>> GetRoleByIdAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/roles/{roleId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<RoleDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var role = await response.Content.ReadFromJsonAsync<RoleDto>(_jsonOptions);
                return role != null
                    ? Result<RoleDto>.Success(role)
                    : Result<RoleDto>.Failure("Role not found", (List<string>?)null);
            }
            catch (Exception ex)
            {
                return Result<RoleDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<string>> CreateRoleAsync(CreateRoleRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/roles", request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<string>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<CreateRoleResponse>(_jsonOptions);
                return Result<string>.Success(result?.RoleId ?? string.Empty, "Rol başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> UpdateRoleAsync(string roleId, UpdateRoleRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/roles/{roleId}", request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Rol başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> DeleteRoleAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/roles/{roleId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Rol başarıyla silindi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<List<PermissionDto>>> GetRolePermissionsAsync(string roleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/roles/{roleId}/permissions");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<PermissionDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(_jsonOptions);
                return Result<List<PermissionDto>>.Success(permissions ?? new List<PermissionDto>());
            }
            catch (Exception ex)
            {
                return Result<List<PermissionDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> UpdateRolePermissionsAsync(string roleId, List<int> permissionIds)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/roles/{roleId}/permissions", permissionIds, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Yetkiler başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(string? module = null)
        {
            try
            {
                var url = "api/roles/permissions";
                if (!string.IsNullOrEmpty(module))
                    url += $"?module={Uri.EscapeDataString(module)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<PermissionDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var permissions = await response.Content.ReadFromJsonAsync<List<PermissionDto>>(_jsonOptions);
                return Result<List<PermissionDto>>.Success(permissions ?? new List<PermissionDto>());
            }
            catch (Exception ex)
            {
                return Result<List<PermissionDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        private class RolesResponse
        {
            public List<RoleDto> Roles { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        private class CreateRoleResponse
        {
            public string RoleId { get; set; } = string.Empty;
        }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}
