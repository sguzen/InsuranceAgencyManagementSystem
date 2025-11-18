using IAMS.Application.DTOs.Identity;
using IAMS.Application.Models;

namespace IAMS.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<Result<List<UserDto>>> GetAllUsersAsync();
        Task<Result<UserDto>> GetUserByIdAsync(string userId);
        Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto);
        Task<Result<UserDto>> UpdateUserAsync(UpdateUserDto dto);
        Task<Result> DeleteUserAsync(string userId);
        Task<Result> ChangePasswordAsync(string userId, string newPassword);
        Task<Result> ToggleUserStatusAsync(string userId, bool isActive);
        Task<Result<List<string>>> GetAllRolesAsync();
        Task<Result<List<string>>> GetUserRolesAsync(string userId);
        Task<Result> UpdateUserRolesAsync(string userId, List<string> roles);
    }
}
