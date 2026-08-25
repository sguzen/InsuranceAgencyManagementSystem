using IAMS.Shared.DTOs.Identity;
using IAMS.Domain.Entities;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserManagement")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPermissionService _permissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IPermissionService permissionService,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _permissionService = permissionService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var query = _roleManager.Roles.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(r =>
                        r.Name!.ToLower().Contains(search) ||
                        (r.Description != null && r.Description.ToLower().Contains(search)));
                }

                var totalCount = await query.CountAsync();
                var roles = await query
                    .OrderBy(r => r.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var roleDtos = new List<RoleDto>();
                foreach (var role in roles)
                {
                    var permissions = await _permissionService.GetRolePermissionsAsync(role.Id);
                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name!,
                        Description = role.Description ?? string.Empty,
                        IsDefault = role.IsDefault,
                        IsSystem = role.IsSystem,
                        Permissions = permissions
                    });
                }

                return Ok(new
                {
                    roles = roleDtos,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { message = "Error retrieving roles" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                var permissions = await _permissionService.GetRolePermissionsAsync(role.Id);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Description = role.Description ?? string.Empty,
                    IsDefault = role.IsDefault,
                    IsSystem = role.IsSystem,
                    Permissions = permissions
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role {RoleId}", id);
                return StatusCode(500, new { message = "Error retrieving role" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if role name already exists
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null)
                {
                    return BadRequest(new { message = "Role name already exists" });
                }

                var role = new ApplicationRole
                {
                    Name = request.Name,
                    Description = request.Description,
                    IsDefault = request.IsDefault,
                    IsSystem = false, // New roles are never system roles
                    CreatedOn = DateTime.UtcNow
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                // Assign permissions if provided
                if (request.PermissionIds?.Any() == true)
                {
                    await _permissionService.UpdateRolePermissionsAsync(role.Id, request.PermissionIds);
                }

                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, new { roleId = role.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { message = "Error creating role" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Prevent modification of system roles
                if (role.IsSystem)
                {
                    return BadRequest(new { message = "System roles cannot be modified" });
                }

                // Check if new name conflicts with existing role (excluding current role)
                if (role.Name != request.Name)
                {
                    var existingRole = await _roleManager.FindByNameAsync(request.Name);
                    if (existingRole != null && existingRole.Id != id)
                    {
                        return BadRequest(new { message = "Role name already exists" });
                    }
                }

                role.Name = request.Name;
                role.Description = request.Description;
                role.IsDefault = request.IsDefault;

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                // Update permissions
                await _permissionService.UpdateRolePermissionsAsync(role.Id, request.PermissionIds);

                return Ok(new { message = "Role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(500, new { message = "Error updating role" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Prevent deletion of system roles     
                if (role.IsSystem)
                {
                    return BadRequest(new { message = "System roles cannot be deleted" });
                }

                // Check if role is assigned to any users
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                if (usersInRole.Any())
                {
                    return BadRequest(new { message = "Cannot delete role that is assigned to users" });
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }

                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(500, new { message = "Error deleting role" });
            }
        }

        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                var permissions = await _permissionService.GetRolePermissionsAsync(id);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role permissions for role {RoleId}", id);
                return StatusCode(500, new { message = "Error retrieving role permissions" });
            }
        }

        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(string id, [FromBody] List<int> permissionIds)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Prevent modification of system role permissions
                if (role.IsSystem)
                {
                    return BadRequest(new { message = "System role permissions cannot be modified" });
                }

                await _permissionService.UpdateRolePermissionsAsync(id, permissionIds);
                return Ok(new { message = "Role permissions updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permissions for role {RoleId}", id);
                return StatusCode(500, new { message = "Error updating role permissions" });
            }
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions([FromQuery] string? module = null)
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();

                if (!string.IsNullOrEmpty(module))
                {
                    permissions = permissions.Where(p =>
                        string.Equals(p.Module, module, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, new { message = "Error retrieving permissions" });
            }
        }

        public class CreateRoleDto
        {
            [Required(ErrorMessage = "Role name is required")]
            [StringLength(256, ErrorMessage = "Role name cannot exceed 256 characters")]
            public string Name { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string? Description { get; set; }

            public bool IsDefault { get; set; } = false;

            public List<int> PermissionIds { get; set; } = new();
        }
        public class UpdateRoleDto
        {
            [Required(ErrorMessage = "Role name is required")]
            [StringLength(256, ErrorMessage = "Role name cannot exceed 256 characters")]
            public string Name { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string? Description { get; set; }

            public bool IsDefault { get; set; }

            public List<int> PermissionIds { get; set; } = new();
        }
    }
}