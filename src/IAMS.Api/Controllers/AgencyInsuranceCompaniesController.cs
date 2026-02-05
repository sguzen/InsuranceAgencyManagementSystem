using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Agency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IAMS.Persistence.Contexts;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/agencies/{agencyId}/insurance-companies")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class AgencyInsuranceCompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AgencyInsuranceCompaniesController> _logger;

        public AgencyInsuranceCompaniesController(
            ApplicationDbContext context,
            ILogger<AgencyInsuranceCompaniesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencyInsuranceCompanies(int agencyId)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(agencyId);
                if (tenant == null)
                    return NotFound(new { message = "Agency not found" });

                var associations = await _context.TenantInsuranceCompanies
                    .Include(tic => tic.InsuranceCompany)
                    .Where(tic => tic.TenantId == agencyId && !tic.IsDeleted)
                    .OrderBy(tic => tic.InsuranceCompany.Name)
                    .ToListAsync();

                var dtos = associations.Select(MapToDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance companies for agency {AgencyId}", agencyId);
                return StatusCode(500, new { message = "Error retrieving insurance companies" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgencyInsuranceCompany(int agencyId, int id)
        {
            try
            {
                var association = await _context.TenantInsuranceCompanies
                    .Include(tic => tic.InsuranceCompany)
                    .FirstOrDefaultAsync(tic => tic.Id == id && tic.TenantId == agencyId && !tic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                return Ok(MapToDto(association));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance company association {Id} for agency {AgencyId}", id, agencyId);
                return StatusCode(500, new { message = "Error retrieving insurance company association" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddInsuranceCompany(int agencyId, [FromBody] CreateAgencyInsuranceCompanyDto request)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(agencyId);
                if (tenant == null)
                    return NotFound(new { message = "Agency not found" });

                var insuranceCompany = await _context.InsuranceCompanies.FindAsync(request.InsuranceCompanyId);
                if (insuranceCompany == null)
                    return NotFound(new { message = "Insurance company not found" });

                // Check for existing association (including soft-deleted)
                var existingActive = await _context.TenantInsuranceCompanies
                    .FirstOrDefaultAsync(tic => tic.TenantId == agencyId
                        && tic.InsuranceCompanyId == request.InsuranceCompanyId
                        && !tic.IsDeleted);
                if (existingActive != null)
                    return BadRequest(new { message = "This insurance company is already associated with this agency" });

                // Check for soft-deleted association and reactivate it
                var existingDeleted = await _context.TenantInsuranceCompanies
                    .FirstOrDefaultAsync(tic => tic.TenantId == agencyId
                        && tic.InsuranceCompanyId == request.InsuranceCompanyId
                        && tic.IsDeleted);

                TenantInsuranceCompany association;
                if (existingDeleted != null)
                {
                    // Reactivate the soft-deleted record
                    existingDeleted.Restore();
                    existingDeleted.DbServer = request.DbServer;
                    existingDeleted.DbName = request.DbName;
                    existingDeleted.DbUsername = request.DbUsername;
                    existingDeleted.DbPassword = request.DbPassword;
                    existingDeleted.ConnectionString = request.ConnectionString;
                    existingDeleted.Notes = request.Notes;
                    existingDeleted.IsActive = true;
                    existingDeleted.ModifiedBy = "admin";
                    existingDeleted.ModifiedOn = DateTime.UtcNow;
                    association = existingDeleted;
                }
                else
                {
                    association = new TenantInsuranceCompany
                    {
                        TenantId = agencyId,
                        InsuranceCompanyId = request.InsuranceCompanyId,
                        DbServer = request.DbServer,
                        DbName = request.DbName,
                        DbUsername = request.DbUsername,
                        DbPassword = request.DbPassword,
                        ConnectionString = request.ConnectionString,
                        Notes = request.Notes,
                        IsActive = true,
                        CreatedBy = "admin",
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.TenantInsuranceCompanies.Add(association);
                }

                await _context.SaveChangesAsync();

                // Reload with navigation property
                await _context.Entry(association).Reference(a => a.InsuranceCompany).LoadAsync();

                return CreatedAtAction(
                    nameof(GetAgencyInsuranceCompany),
                    new { agencyId, id = association.Id },
                    MapToDto(association));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding insurance company to agency {AgencyId}", agencyId);
                return StatusCode(500, new { message = "Error adding insurance company" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInsuranceCompany(int agencyId, int id, [FromBody] UpdateAgencyInsuranceCompanyDto request)
        {
            try
            {
                var association = await _context.TenantInsuranceCompanies
                    .FirstOrDefaultAsync(tic => tic.Id == id && tic.TenantId == agencyId && !tic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                association.DbServer = request.DbServer;
                association.DbName = request.DbName;
                association.DbUsername = request.DbUsername;
                // Only update password if a new one was provided
                if (!string.IsNullOrEmpty(request.DbPassword))
                    association.DbPassword = request.DbPassword;
                association.ConnectionString = request.ConnectionString;
                association.IsActive = request.IsActive;
                association.Notes = request.Notes;
                association.ModifiedBy = "admin";
                association.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Insurance company association updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance company association {Id} for agency {AgencyId}", id, agencyId);
                return StatusCode(500, new { message = "Error updating insurance company association" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveInsuranceCompany(int agencyId, int id)
        {
            try
            {
                var association = await _context.TenantInsuranceCompanies
                    .FirstOrDefaultAsync(tic => tic.Id == id && tic.TenantId == agencyId && !tic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                association.MarkAsDeleted("admin");
                await _context.SaveChangesAsync();

                return Ok(new { message = "Insurance company removed from agency" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing insurance company {Id} from agency {AgencyId}", id, agencyId);
                return StatusCode(500, new { message = "Error removing insurance company" });
            }
        }

        /// <summary>
        /// Returns all insurance companies (for dropdown selection when adding a new association).
        /// </summary>
        [HttpGet("/api/insurance-companies/all")]
        public async Task<IActionResult> GetAllInsuranceCompanies()
        {
            try
            {
                var companies = await _context.InsuranceCompanies
                    .Where(ic => !ic.IsDeleted)
                    .OrderBy(ic => ic.Name)
                    .Select(ic => new InsuranceCompanySummaryDto
                    {
                        Id = ic.Id,
                        Name = ic.Name,
                        Code = ic.Code,
                        IsActive = ic.IsActive
                    })
                    .ToListAsync();

                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all insurance companies");
                return StatusCode(500, new { message = "Error retrieving insurance companies" });
            }
        }

        [HttpPost("{id}/test-connection")]
        public async Task<IActionResult> TestConnection(int agencyId, int id)
        {
            try
            {
                var association = await _context.TenantInsuranceCompanies
                    .FirstOrDefaultAsync(tic => tic.Id == id && tic.TenantId == agencyId && !tic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                var connectionString = association.ConnectionString;
                if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(association.DbServer))
                {
                    connectionString = $"Server={association.DbServer};Database={association.DbName};User Id={association.DbUsername};Password={association.DbPassword};TrustServerCertificate=True;";
                }

                if (string.IsNullOrEmpty(connectionString))
                    return BadRequest(new { message = "No connection information configured" });

                // Test the connection
                using var testConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await testConnection.OpenAsync();
                await testConnection.CloseAsync();

                return Ok(new { message = "Connection successful", success = true });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                return Ok(new { message = $"Connection failed: {ex.Message}", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for association {Id} agency {AgencyId}", id, agencyId);
                return Ok(new { message = $"Connection failed: {ex.Message}", success = false });
            }
        }

        private static AgencyInsuranceCompanyDto MapToDto(TenantInsuranceCompany association)
        {
            return new AgencyInsuranceCompanyDto
            {
                Id = association.Id,
                TenantId = association.TenantId,
                InsuranceCompanyId = association.InsuranceCompanyId,
                InsuranceCompanyName = association.InsuranceCompany?.Name ?? string.Empty,
                InsuranceCompanyCode = association.InsuranceCompany?.Code ?? string.Empty,
                DbServer = association.DbServer,
                DbName = association.DbName,
                DbUsername = association.DbUsername,
                HasPassword = !string.IsNullOrEmpty(association.DbPassword),
                ConnectionString = association.ConnectionString,
                IsActive = association.IsActive,
                Notes = association.Notes,
                CreatedOn = association.CreatedOn,
                ModifiedOn = association.ModifiedOn
            };
        }
    }
}
