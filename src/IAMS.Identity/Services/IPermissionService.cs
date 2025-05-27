using IAMS.Application.DTOs.Identity;

namespace IAMS.Identity.Services
{
    public interface IPermissionService
    {
        Task<List<PermissionDto>> GetAllPermissionsAsync();
        Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId);
        Task UpdateRolePermissionsAsync(string roleId, List<int> permissionIds);
        Task<List<string>> GetUserPermissionsAsync(string userId);
    }
}