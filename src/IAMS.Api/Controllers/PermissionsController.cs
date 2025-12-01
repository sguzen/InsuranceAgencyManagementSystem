using IAMS.Application.DTOs.Identity;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IUnitOfWork unitOfWork,
            ILogger<PermissionsController> logger)
        {
            _unitOfWork = unitOfWork;
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
                var pagedResult = await _unitOfWork.Permissions.GetPermissionsPagedAsync(
                    page,
                    pageSize,
                    module);

                var permissions = pagedResult.Items.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Module = p.Module,
                    IsSystem = p.IsSystem
                }).ToList();

                return Ok(new
                {
                    permissions,
                    totalCount = pagedResult.TotalCount,
                    page = pagedResult.PageNumber,
                    pageSize = pagedResult.PageSize,
                    totalPages = pagedResult.TotalPages
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
                var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
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
                if (await _unitOfWork.Permissions.NameExistsAsync(request.Name))
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

                await _unitOfWork.Permissions.AddAsync(permission);
                await _unitOfWork.SaveChangesAsync();

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

                var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
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
                    if (await _unitOfWork.Permissions.NameExistsAsync(request.Name, id))
                    {
                        return BadRequest(new { message = "Permission name already exists" });
                    }
                }

                permission.Name = request.Name;
                permission.DisplayName = request.DisplayName;
                permission.Description = request.Description ?? string.Empty;
                permission.Module = request.Module;

                _unitOfWork.Permissions.Update(permission);
                await _unitOfWork.SaveChangesAsync();

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
                var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
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
                if (await _unitOfWork.Permissions.IsAssignedToRolesAsync(id))
                {
                    return BadRequest(new { message = "Cannot delete permission that is assigned to roles" });
                }

                _unitOfWork.Permissions.Remove(permission);
                await _unitOfWork.SaveChangesAsync();

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
                var modules = await _unitOfWork.Permissions.GetModulesAsync();
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
                var addedCount = await _unitOfWork.Permissions.SeedDefaultPermissionsAsync();
                await _unitOfWork.SaveChangesAsync();

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
