using IAMS.Domain.Enums;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.Shared.DTOs.Agency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class AgenciesController : ControllerBase
    {
        private readonly TenantDbContext _context;
        private readonly ILogger<AgenciesController> _logger;

        public AgenciesController(
            TenantDbContext context,
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
            [FromQuery] AgencyStatus? status = null)
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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                return Ok(MapToDto(agency));
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

                var agency = new TenantEntity
                {
                    Name = request.Name,
                    Identifier = request.Identifier,
                    ExternalId = request.ExternalId,
                    SubscriptionType = request.SubscriptionType,
                    SubscriptionPlan = request.SubscriptionType.ToString(),
                    ContactEmail = request.ContactEmail ?? string.Empty,
                    ContactPhone = request.ContactPhone ?? string.Empty,
                    MaxUsers = request.MaxUsers,
                    MaxPolicies = request.MaxPolicies,
                    SubscriptionExpiry = request.SubscriptionExpiryDate,
                    Status = AgencyStatus.Active,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    ConnectionString = string.Empty, // Will be set during setup
                    TimeZone = "Europe/Istanbul",
                    Currency = "TRY",
                    Language = "tr"
                };

                _context.Tenants.Add(agency);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAgency), new { id = agency.Id }, MapToDto(agency));
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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                // Check if identifier conflicts with another agency
                if (agency.Identifier.ToLower() != request.Identifier.ToLower())
                {
                    var existing = await _context.Tenants
                        .AnyAsync(t => t.Identifier.ToLower() == request.Identifier.ToLower() && t.Id != id);
                    if (existing)
                    {
                        return BadRequest(new { message = "Agency identifier already exists" });
                    }
                }

                agency.Name = request.Name;
                agency.Identifier = request.Identifier;
                agency.ExternalId = request.ExternalId;
                agency.Status = request.Status;
                agency.IsActive = request.Status == AgencyStatus.Active;
                agency.SubscriptionType = request.SubscriptionType;
                agency.SubscriptionPlan = request.SubscriptionType.ToString();
                agency.ContactEmail = request.ContactEmail ?? string.Empty;
                agency.ContactPhone = request.ContactPhone ?? string.Empty;
                agency.MaxUsers = request.MaxUsers;
                agency.MaxPolicies = request.MaxPolicies;
                agency.SubscriptionExpiry = request.SubscriptionExpiryDate;
                agency.LastUpdated = DateTime.UtcNow;

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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                // Soft delete - suspend the agency
                agency.Status = AgencyStatus.Suspended;
                agency.IsActive = false;
                agency.LastUpdated = DateTime.UtcNow;
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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                agency.Status = AgencyStatus.Active;
                agency.IsActive = true;
                agency.LastUpdated = DateTime.UtcNow;
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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                agency.Status = AgencyStatus.Suspended;
                agency.IsActive = false;
                agency.LastUpdated = DateTime.UtcNow;
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
                var agency = await _context.Tenants.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new { message = "Agency not found" });
                }

                agency.SubscriptionType = request.SubscriptionType;
                agency.SubscriptionPlan = request.SubscriptionType.ToString();
                agency.SubscriptionExpiry = request.ExpiryDate;
                agency.LastUpdated = DateTime.UtcNow;

                // Update limits based on subscription type
                switch (request.SubscriptionType)
                {
                    case SubscriptionType.Basic:
                        agency.MaxUsers = 5;
                        agency.MaxPolicies = 1000;
                        break;
                    case SubscriptionType.Standard:
                        agency.MaxUsers = 15;
                        agency.MaxPolicies = 5000;
                        break;
                    case SubscriptionType.Premium:
                        agency.MaxUsers = 50;
                        agency.MaxPolicies = 20000;
                        break;
                    case SubscriptionType.Enterprise:
                        agency.MaxUsers = 999;
                        agency.MaxPolicies = 999999;
                        break;
                }

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
                var activeAgencies = await _context.Tenants.CountAsync(t => t.Status == AgencyStatus.Active);
                var suspendedAgencies = await _context.Tenants.CountAsync(t => t.Status == AgencyStatus.Suspended);
                var trialAgencies = await _context.Tenants.CountAsync(t => t.Status == AgencyStatus.Trial);

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

        private static AgencyDto MapToDto(TenantEntity agency)
        {
            return new AgencyDto
            {
                Id = agency.Id,
                Name = agency.Name,
                Identifier = agency.Identifier,
                ExternalId = agency.ExternalId,
                Status = agency.Status,
                SubscriptionType = agency.SubscriptionType,
                CreatedOn = agency.CreatedOn,
                TrialExpiryDate = agency.TrialExpiryDate,
                SubscriptionExpiryDate = agency.SubscriptionExpiry,
                ContactEmail = agency.ContactEmail,
                ContactPhone = agency.ContactPhone,
                MaxUsers = agency.MaxUsers,
                MaxPolicies = agency.MaxPolicies,
                ModuleSettings = agency.ModuleSettings,
                IsActive = agency.IsActive && agency.Status == AgencyStatus.Active,
                IsExpired = agency.SubscriptionExpiry.HasValue && agency.SubscriptionExpiry < DateTime.Today
            };
        }
    }

    public class UpdateSubscriptionDto
    {
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
