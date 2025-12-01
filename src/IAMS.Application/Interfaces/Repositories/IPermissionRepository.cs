using IAMS.Application.Models;
using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// Gets paginated permissions with optional module filter
        /// </summary>
        Task<PagedResult<Permission>> GetPermissionsPagedAsync(
            int page,
            int pageSize,
            string? module = null);

        /// <summary>
        /// Gets a permission by name
        /// </summary>
        Task<Permission?> GetByNameAsync(string name);

        /// <summary>
        /// Checks if a permission name exists, optionally excluding a specific permission
        /// </summary>
        Task<bool> NameExistsAsync(string name, int? excludePermissionId = null);

        /// <summary>
        /// Checks if a permission is assigned to any roles
        /// </summary>
        Task<bool> IsAssignedToRolesAsync(int permissionId);

        /// <summary>
        /// Gets distinct list of modules
        /// </summary>
        Task<List<string>> GetModulesAsync();

        /// <summary>
        /// Seeds default system permissions
        /// </summary>
        Task<int> SeedDefaultPermissionsAsync();
    }
}
