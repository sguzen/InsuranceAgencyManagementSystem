using IAMS.Application.DTOs.Identity;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            ApplicationDbContext context,
            ILogger<PermissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions(
            [FromQuery] string? module = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Permissions.AsQueryable();

                if (!string.IsNullOrEmpty(module))
                {
                    query = query.Where(p => p.Module == module);
                }

                var totalCount = await query.CountAsync();
                var permissions = await query
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.DisplayName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Description = p.Description,
                        Module = p.Module,
                        IsSystem = p.IsSystem
                    })
                    .ToListAsync();

                return Ok(new
                {
                    permissions,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, new { message = "Error retrieving permissions" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermission(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound(new { message = "Permission not found" });
                }

                var permissionDto = new PermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    DisplayName = permission.DisplayName,
                    Description = permission.Description,
                    Module = permission.Module,
                    IsSystem = permission.IsSystem
                };

                return Ok(permissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error retrieving permission" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if permission name already exists
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == request.Name);
                if (existingPermission != null)
                {
                    return BadRequest(new { message = "Permission name already exists" });
                }

                var permission = new Permission
                {
                    Name = request.Name,
                    DisplayName = request.DisplayName,
                    Description = request.Description ?? string.Empty,
                    Module = request.Module,
                    IsSystem = false // New permissions are never system permissions
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPermission), new { id = permission.Id },
                    new { permissionId = permission.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, new { message = "Error creating permission" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound(new { message = "Permission not found" });
                }

                // Prevent modification of system permissions
                if (permission.IsSystem)
                {
                    return BadRequest(new { message = "System permissions cannot be modified" });
                }

                // Check if new name conflicts with existing permission (excluding current permission)
                if (permission.Name != request.Name)
                {
                    var existingPermission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.Name == request.Name && p.Id != id);
                    if (existingPermission != null)
                    {
                        return BadRequest(new { message = "Permission name already exists" });
                    }
                }

                permission.Name = request.Name;
                permission.DisplayName = request.DisplayName;
                permission.Description = request.Description ?? string.Empty;
                permission.Module = request.Module;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Permission updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error updating permission" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound(new { message = "Permission not found" });
                }

                // Prevent deletion of system permissions
                if (permission.IsSystem)
                {
                    return BadRequest(new { message = "System permissions cannot be deleted" });
                }

                // Check if permission is assigned to any roles
                var isAssignedToRoles = await _context.RolePermissions
                    .AnyAsync(rp => rp.PermissionId == id);
                if (isAssignedToRoles)
                {
                    return BadRequest(new { message = "Cannot delete permission that is assigned to roles" });
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Permission deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
                return StatusCode(500, new { message = "Error deleting permission" });
            }
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            try
            {
                var modules = await _context.Permissions
                    .Where(p => !string.IsNullOrEmpty(p.Module))
                    .Select(p => p.Module)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving modules");
                return StatusCode(500, new { message = "Error retrieving modules" });
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedDefaultPermissions()
        {
            try
            {
                // Define default permissions for your modules
                var defaultPermissions = new[]
                {
                    // Core module permissions
                    new Permission { Name = "customers.view", DisplayName = "View Customers", Description = "Can view customer list and details", Module = "Core", IsSystem = true },
                    new Permission { Name = "customers.create", DisplayName = "Create Customers", Description = "Can create new customers", Module = "Core", IsSystem = true },
                    new Permission { Name = "customers.edit", DisplayName = "Edit Customers", Description = "Can edit customer information", Module = "Core", IsSystem = true },
                    new Permission { Name = "customers.delete", DisplayName = "Delete Customers", Description = "Can delete customers", Module = "Core", IsSystem = true },

                    new Permission { Name = "policies.view", DisplayName = "View Policies", Description = "Can view policy list and details", Module = "Core", IsSystem = true },
                    new Permission { Name = "policies.create", DisplayName = "Create Policies", Description = "Can create new policies", Module = "Core", IsSystem = true },
                    new Permission { Name = "policies.edit", DisplayName = "Edit Policies", Description = "Can edit policy information", Module = "Core", IsSystem = true },
                    new Permission { Name = "policies.delete", DisplayName = "Delete Policies", Description = "Can delete policies", Module = "Core", IsSystem = true },

                    // User management permissions
                    new Permission { Name = "users.view", DisplayName = "View Users", Description = "Can view user list and details", Module = "Core", IsSystem = true },
                    new Permission { Name = "users.create", DisplayName = "Create Users", Description = "Can create new users", Module = "Core", IsSystem = true },
                    new Permission { Name = "users.edit", DisplayName = "Edit Users", Description = "Can edit user information", Module = "Core", IsSystem = true },
                    new Permission { Name = "users.delete", DisplayName = "Delete Users", Description = "Can delete users", Module = "Core", IsSystem = true },

                    new Permission { Name = "roles.view", DisplayName = "View Roles", Description = "Can view role list and details", Module = "Core", IsSystem = true },
                    new Permission { Name = "roles.create", DisplayName = "Create Roles", Description = "Can create new roles", Module = "Core", IsSystem = true },
                    new Permission { Name = "roles.edit", DisplayName = "Edit Roles", Description = "Can edit role information", Module = "Core", IsSystem = true },
                    new Permission { Name = "roles.delete", DisplayName = "Delete Roles", Description = "Can delete roles", Module = "Core", IsSystem = true },

                    // Reporting module permissions
                    new Permission { Name = "reports.view", DisplayName = "View Reports", Description = "Can view reports", Module = "Reporting", IsSystem = true },
                    new Permission { Name = "reports.create", DisplayName = "Create Reports", Description = "Can create custom reports", Module = "Reporting", IsSystem = true },
                    new Permission { Name = "reports.export", DisplayName = "Export Reports", Description = "Can export reports to various formats", Module = "Reporting", IsSystem = true },

                    // Accounting module permissions
                    new Permission { Name = "accounting.view", DisplayName = "View Accounting", Description = "Can view accounting information", Module = "Accounting", IsSystem = true },
                    new Permission { Name = "accounting.manage", DisplayName = "Manage Accounting", Description = "Can manage accounting entries", Module = "Accounting", IsSystem = true },
                    new Permission { Name = "commissions.view", DisplayName = "View Commissions", Description = "Can view commission reports", Module = "Accounting", IsSystem = true },
                    new Permission { Name = "commissions.manage", DisplayName = "Manage Commissions", Description = "Can manage commission calculations", Module = "Accounting", IsSystem = true },

                    // Integration module permissions
                    new Permission { Name = "integrations.view", DisplayName = "View Integrations", Description = "Can view integration status", Module = "Integration", IsSystem = true },
                    new Permission { Name = "integrations.manage", DisplayName = "Manage Integrations", Description = "Can configure and manage integrations", Module = "Integration", IsSystem = true },
                    new Permission { Name = "mappings.manage", DisplayName = "Manage ID Mappings", Description = "Can manage customer ID mappings", Module = "Integration", IsSystem = true },

                    // Admin permissions
                    new Permission { Name = "admin.system", DisplayName = "System Administration", Description = "Full system administration access", Module = "Admin", IsSystem = true },
                    new Permission { Name = "admin.tenants", DisplayName = "Tenant Management", Description = "Can manage tenant settings", Module = "Admin", IsSystem = true },
                    new Permission { Name = "admin.modules", DisplayName = "Module Management", Description = "Can enable/disable modules", Module = "Admin", IsSystem = true }
                };

                foreach (var permission in defaultPermissions)
                {
                    var exists = await _context.Permissions
                        .AnyAsync(p => p.Name == permission.Name);

                    if (!exists)
                    {
                        _context.Permissions.Add(permission);
                    }
                }

                var addedCount = await _context.SaveChangesAsync();

                return Ok(new { message = $"Seeded {addedCount} default permissions successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default permissions");
                return StatusCode(500, new { message = "Error seeding default permissions" });
            }
        }
    }

    public class CreatePermissionDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Module { get; set; }
    }

    public class UpdatePermissionDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Module { get; set; }
    }
}