using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Agency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IAMS.Persistence.Contexts;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class AgenciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AgenciesController> _logger;

        public AgenciesController(
            ApplicationDbContext context,
            ILogger<AgenciesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencies(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] TenantStatus? status = null)
        {
            try
            {
                var query = _context.Tenants.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(t =>
                        t.Name.ToLower().Contains(search) ||
                        t.Identifier.ToLower().Contains(search) ||
                        (t.ContactEmail != null && t.ContactEmail.ToLower().Contains(search)));
                }

                if (status.HasValue)
                {
                    query = query.Where(t => t.Status == status.Value);
                }

                var totalCount = await query.CountAsync();
                var agencies = await query
                    .OrderBy(t => t.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var agencyDtos = agencies.Select(t => MapToDto(t)).ToList();

                return Ok(new
                {
                    agencies = agencyDtos,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agencies");
                return StatusCode(500, new { message = "Error retrieving agencies" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgency(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                return Ok(MapToDto(tenant));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error retrieving agency" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromBody] CreateAgencyDto request)
        {
            try
            {
                // Check if identifier already exists
                var existing = await _context.Tenants
                    .AnyAsync(t => t.Identifier.ToLower() == request.Identifier.ToLower());
                if (existing)
                {
                    return BadRequest(new { message = "Agency identifier already exists" });
                }

                var tenant = new Tenant
                {
                    Name = request.Name,
                    Identifier = request.Identifier,
                    ExternalId = request.ExternalId,
                    SubscriptionType = request.SubscriptionType,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    MaxUsers = request.MaxUsers,
                    MaxPolicies = request.MaxPolicies,
                    SubscriptionExpiryDate = request.SubscriptionExpiryDate,
                    Status = TenantStatus.Active,
                    CreatedOn = DateTime.UtcNow
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAgency), new { id = tenant.Id }, MapToDto(tenant));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating agency");
                return StatusCode(500, new { message = "Error creating agency" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgency(int id, [FromBody] UpdateAgencyDto request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                // Check if identifier conflicts with another agency
                if (tenant.Identifier.ToLower() != request.Identifier.ToLower())
                {
                    var existing = await _context.Tenants
                        .AnyAsync(t => t.Identifier.ToLower() == request.Identifier.ToLower() && t.Id != id);
                    if (existing)
                    {
                        return BadRequest(new { message = "Agency identifier already exists" });
                    }
                }

                tenant.Name = request.Name;
                tenant.Identifier = request.Identifier;
                tenant.ExternalId = request.ExternalId;
                tenant.Status = request.Status;
                tenant.SubscriptionType = request.SubscriptionType;
                tenant.ContactEmail = request.ContactEmail;
                tenant.ContactPhone = request.ContactPhone;
                tenant.MaxUsers = request.MaxUsers;
                tenant.MaxPolicies = request.MaxPolicies;
                tenant.SubscriptionExpiryDate = request.SubscriptionExpiryDate;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Agency updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error updating agency" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgency(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                // Soft delete - suspend the agency
                tenant.Status = TenantStatus.Suspended;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Agency suspended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error deleting agency" });
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateAgency(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                tenant.ActivateTenant();
                await _context.SaveChangesAsync();

                return Ok(new { message = "Agency activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error activating agency" });
            }
        }

        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendAgency(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                tenant.SuspendTenant();
                await _context.SaveChangesAsync();

                return Ok(new { message = "Agency suspended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error suspending agency" });
            }
        }

        [HttpPut("{id}/subscription")]
        public async Task<IActionResult> UpdateSubscription(int id, [FromBody] UpdateSubscriptionDto request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                tenant.UpgradeSubscription(request.SubscriptionType, request.ExpiryDate);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subscription updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription for agency {AgencyId}", id);
                return StatusCode(500, new { message = "Error updating subscription" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetAgencyStats()
        {
            try
            {
                var totalAgencies = await _context.Tenants.CountAsync();
                var activeAgencies = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Active);
                var suspendedAgencies = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Suspended);
                var trialAgencies = await _context.Tenants.CountAsync(t => t.Status == TenantStatus.Trial);

                var bySubscription = await _context.Tenants
                    .GroupBy(t => t.SubscriptionType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    totalAgencies,
                    activeAgencies,
                    suspendedAgencies,
                    trialAgencies,
                    bySubscription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agency stats");
                return StatusCode(500, new { message = "Error retrieving agency stats" });
            }
        }

        private static AgencyDto MapToDto(Tenant tenant)
        {
            return new AgencyDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                ExternalId = tenant.ExternalId,
                Status = tenant.Status,
                SubscriptionType = tenant.SubscriptionType,
                CreatedOn = tenant.CreatedOn,
                TrialExpiryDate = tenant.TrialExpiryDate,
                SubscriptionExpiryDate = tenant.SubscriptionExpiryDate,
                ContactEmail = tenant.ContactEmail,
                ContactPhone = tenant.ContactPhone,
                MaxUsers = tenant.MaxUsers,
                MaxPolicies = tenant.MaxPolicies,
                ModuleSettings = tenant.ModuleSettings,
                IsActive = tenant.IsActive,
                IsExpired = tenant.IsExpired
            };
        }
    }

    public class UpdateSubscriptionDto
    {
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
