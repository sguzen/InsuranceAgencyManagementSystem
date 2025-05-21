using IAMS.Application.DTOs.Identity;
using IAMS.Identity.Models;
using IAMS.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IPermissionService permissionService,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            return Ok(roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsDefault = r.IsDefault,
                IsSystem = r.IsSystem
            }));
        }

        [HttpGet("{roleId}/permissions")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetRolePermissions(string roleId)
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }

        [HttpPut("{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsDto updateDto)
        {
            try
            {
                await _permissionService.UpdateRolePermissionsAsync(roleId, updateDto.PermissionIds);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role permissions for role {RoleId}", roleId);
                return StatusCode(500, new { Message = "An error occurred while updating role permissions" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createDto)
        {
            // Validate role name doesn't exist
            var existingRole = await _roleManager.FindByNameAsync(createDto.Name);

            if (existingRole != null)
            {
                return BadRequest(new { Message = "Role name already exists" });
            }

            // Create new role
            var role = new ApplicationRole
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsDefault = createDto.IsDefault,
                IsSystem = false,
                TenantId = GetTenantId(),
                NormalizedName = createDto.Name.ToUpper()
            };

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // If this is set as default, unset any other default roles
            if (role.IsDefault)
            {
                await UnsetOtherDefaultRolesAsync(role.Id);
            }

            // Add permissions if specified
            if (createDto.PermissionIds?.Count > 0)
            {
                await _permissionService.UpdateRolePermissionsAsync(role.Id, createDto.PermissionIds);
            }

            return CreatedAtAction(nameof(GetRoleById), new { roleId = role.Id }, new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsDefault = role.IsDefault,
                IsSystem = role.IsSystem
            });
        }

        [HttpGet("{roleId}")]
        public async Task<ActionResult<RoleDto>> GetRoleById(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound();
            }

            return Ok(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsDefault = role.IsDefault,
                IsSystem = role.IsSystem
            });
        }

        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleDto updateDto)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound();
            }

            // Prevent modifying system roles
            if (role.IsSystem)
            {
                return BadRequest(new { Message = "Cannot modify system role" });
            }

            // Update role properties
            role.Description = updateDto.Description;

            // Handle default role changes
            if (updateDto.IsDefault && !role.IsDefault)
            {
                role.IsDefault = true;
                await UnsetOtherDefaultRolesAsync(role.Id);
            }
            else if (!updateDto.IsDefault)
            {
                role.IsDefault = false;
            }

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound();
            }

            // Prevent deleting system roles
            if (role.IsSystem)
            {
                return BadRequest(new { Message = "Cannot delete system role" });
            }

            // Check if role is in use
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

            if (usersInRole.Any())
            {
                return BadRequest(new { Message = "Cannot delete role that is assigned to users" });
            }

            // Delete role permissions first
            await _permissionService.UpdateRolePermissionsAsync(roleId, new List<int>());

            // Delete role
            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return NoContent();
        }

        private async Task UnsetOtherDefaultRolesAsync(string currentRoleId)
        {
            var defaultRoles = await _roleManager.Roles
                .Where(r => r.IsDefault && r.Id != currentRoleId)
                .ToListAsync();

            foreach (var role in defaultRoles)
            {
                role.IsDefault = false;
                await _roleManager.UpdateAsync(role);
            }
        }

        private int GetTenantId()
        {
            // Get tenant ID from the current context
            var tenantIdClaim = User.FindFirstValue("tenant_id");
            return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : 1; // Default to 1 if not found
        }
    }
}