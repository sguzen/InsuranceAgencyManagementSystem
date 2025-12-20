using IAMS.Application.DTOs.Identity;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IUsersApiClient
    {
        Task<Result<PagedResult<UserDto>>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<Result<UserDto>> GetUserByIdAsync(string userId);
        Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<Result> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
        Task<Result> DeleteUserAsync(string userId);
        Task<Result<List<string>>> GetUserRolesAsync(string userId);
        Task<Result> UpdateUserRolesAsync(string userId, List<string> roles);
    }

    public class UsersApiClient : BaseApiClient, IUsersApiClient
    {
        public UsersApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<PagedResult<UserDto>>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
                queryString += $"&search={Uri.EscapeDataString(search)}";

            return await GetAsync<PagedResult<UserDto>>($"api/users{queryString}");
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string userId)
        {
            return await GetAsync<UserDto>($"api/users/{userId}");
        }

        public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            return await PostAsync<UserDto>("api/users", createUserDto);
        }

        public async Task<Result> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            return await PutAsync($"api/users/{userId}", updateUserDto);
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            return await base.DeleteAsync($"api/users/{userId}");
        }

        public async Task<Result<List<string>>> GetUserRolesAsync(string userId)
        {
            return await GetAsync<List<string>>($"api/users/{userId}/roles");
        }

        public async Task<Result> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            return await PutAsync($"api/users/{userId}/roles", new { Roles = roles });
        }
    }
}
