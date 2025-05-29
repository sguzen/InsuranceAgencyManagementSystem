// Fixed PermissionService.cs
using IAMS.Application.DTOs.Identity;
using IAMS.Identity.Data;
using IAMS.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Identity.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationUser> _roleManager;

        public PermissionService(
            IdentityDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationUser> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _context.Permissions.ToListAsync();

            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                Module = p.Module
            }).ToList();
        }

        public async Task<List<PermissionDto>> GetRolePermissionsAsync(string roleId)
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    DisplayName = rp.Permission.DisplayName,
                    Description = rp.Permission.Description,
                    Module = rp.Permission.Module
                })
                .ToListAsync();

            return permissions;
        }

        public async Task UpdateRolePermissionsAsync(string roleId, List<int> permissionIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found");
            }

            // Remove existing permissions
            var existingPermissions = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
            _context.RolePermissions.RemoveRange(existingPermissions);

            // Add new permissions
            var newRolePermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            await _context.RolePermissions.AddRangeAsync(newRolePermissions);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var permissions = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();

            return permissions;
        }
    }
}