using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.Persistence.Services;
using IAMS.Shared.DTOs.Agency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/agencies/{agencyId}/insurance-companies")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class AgencyInsuranceCompaniesController : ControllerBase
    {
        private readonly TenantDbContext _context;
        private readonly IInsuranceCompanySyncService _syncService;
        private readonly ILogger<AgencyInsuranceCompaniesController> _logger;

        public AgencyInsuranceCompaniesController(
            TenantDbContext context,
            IInsuranceCompanySyncService syncService,
            ILogger<AgencyInsuranceCompaniesController> logger)
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencyInsuranceCompanies(int agencyId)
        {
            try
            {
                var agency = await _context.Tenants.FindAsync(agencyId);
                if (agency == null)
                    return NotFound(new { message = "Agency not found" });

                var associations = await _context.AgencyInsuranceCompanies
                    .Where(aic => aic.AgencyId == agencyId && !aic.IsDeleted)
                    .OrderBy(aic => aic.InsuranceCompanyName)
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
                var association = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.Id == id && aic.AgencyId == agencyId && !aic.IsDeleted);

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
                var agency = await _context.Tenants.FindAsync(agencyId);
                if (agency == null)
                    return NotFound(new { message = "Agency not found" });

                // Check for existing association (including soft-deleted)
                var existingActive = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.AgencyId == agencyId
                        && aic.InsuranceCompanyId == request.InsuranceCompanyId
                        && !aic.IsDeleted);
                if (existingActive != null)
                    return BadRequest(new { message = "This insurance company is already associated with this agency" });

                // Check for soft-deleted association and reactivate it
                var existingDeleted = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.AgencyId == agencyId
                        && aic.InsuranceCompanyId == request.InsuranceCompanyId
                        && aic.IsDeleted);

                AgencyInsuranceCompany association;
                if (existingDeleted != null)
                {
                    // Reactivate the soft-deleted record
                    existingDeleted.IsDeleted = false;
                    existingDeleted.DeletedOn = null;
                    existingDeleted.DeletedBy = null;
                    existingDeleted.InsuranceCompanyName = request.InsuranceCompanyName;
                    existingDeleted.InsuranceCompanyCode = request.InsuranceCompanyCode;
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
                    association = new AgencyInsuranceCompany
                    {
                        AgencyId = agencyId,
                        InsuranceCompanyId = request.InsuranceCompanyId,
                        InsuranceCompanyName = request.InsuranceCompanyName,
                        InsuranceCompanyCode = request.InsuranceCompanyCode,
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
                    _context.AgencyInsuranceCompanies.Add(association);
                }

                await _context.SaveChangesAsync();

                // Sync the insurance company to the tenant database
                try
                {
                    await _syncService.SyncInsuranceCompanyAsync(agencyId, request.InsuranceCompanyId);
                }
                catch (Exception syncEx)
                {
                    _logger.LogError(syncEx, "Failed to sync insurance company {InsuranceCompanyId} to tenant DB for agency {AgencyId}. The association was saved but tenant sync failed.",
                        request.InsuranceCompanyId, agencyId);
                }

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
                var association = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.Id == id && aic.AgencyId == agencyId && !aic.IsDeleted);

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
                var association = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.Id == id && aic.AgencyId == agencyId && !aic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                association.IsDeleted = true;
                association.DeletedOn = DateTime.UtcNow;
                association.DeletedBy = "admin";
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
        /// Returns a list of insurance companies from the master database for selection.
        /// </summary>
        [HttpGet("/api/insurance-companies/all")]
        public async Task<IActionResult> GetAllInsuranceCompanies()
        {
            try
            {
                var companies = await _context.InsuranceCompanies
                    .AsNoTracking()
                    .Where(ic => ic.IsActive)
                    .OrderBy(ic => ic.DisplayOrder)
                    .ThenBy(ic => ic.Name)
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
                _logger.LogError(ex, "Error retrieving insurance companies");
                return StatusCode(500, new { message = "Error retrieving insurance companies" });
            }
        }

        [HttpPost("{id}/test-connection")]
        public async Task<IActionResult> TestConnection(int agencyId, int id)
        {
            try
            {
                var association = await _context.AgencyInsuranceCompanies
                    .FirstOrDefaultAsync(aic => aic.Id == id && aic.AgencyId == agencyId && !aic.IsDeleted);

                if (association == null)
                    return NotFound(new { message = "Association not found" });

                var connectionString = association.ConnectionString;
                var isMySql = false;

                if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(association.DbServer))
                {
                    // Detect MySQL by port (3306, 23306, etc.)
                    isMySql = IsMySqlServer(association.DbServer);

                    if (isMySql)
                    {
                        var (host, port) = ParseHostPort(association.DbServer, 3306);
                        connectionString = $"Server={host};Port={port};Database={association.DbName};" +
                                           $"User Id={association.DbUsername};Password={association.DbPassword};" +
                                           "SslMode=Preferred;";
                    }
                    else
                    {
                        connectionString = $"Server={association.DbServer};Database={association.DbName};" +
                                           $"User Id={association.DbUsername};Password={association.DbPassword};" +
                                           "TrustServerCertificate=True;";
                    }
                }
                else if (!string.IsNullOrEmpty(connectionString))
                {
                    isMySql = connectionString.Contains("SslMode=", StringComparison.OrdinalIgnoreCase)
                           || connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase);
                }

                if (string.IsNullOrEmpty(connectionString))
                    return BadRequest(new { message = "No connection information configured" });

                // Test the connection with the appropriate provider
                if (isMySql)
                {
                    await using var mysqlConn = new MySqlConnector.MySqlConnection(connectionString);
                    await mysqlConn.OpenAsync();
                    await mysqlConn.CloseAsync();
                }
                else
                {
                    using var sqlConn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                    await sqlConn.OpenAsync();
                    await sqlConn.CloseAsync();
                }

                return Ok(new { message = "Connection successful", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for association {Id} agency {AgencyId}", id, agencyId);
                return Ok(new { message = $"Connection failed: {ex.Message}", success = false });
            }
        }

        private static bool IsMySqlServer(string server)
        {
            if (server.Contains(':'))
            {
                var portStr = server.Split(':').Last();
                if (int.TryParse(portStr, out var port))
                    return port == 3306 || port % 10000 == 3306;
            }
            return false;
        }

        private static (string Host, int Port) ParseHostPort(string server, int defaultPort)
        {
            var parts = server.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var port))
                return (parts[0], port);
            return (server, defaultPort);
        }

        private static AgencyInsuranceCompanyDto MapToDto(AgencyInsuranceCompany association)
        {
            return new AgencyInsuranceCompanyDto
            {
                Id = association.Id,
                AgencyId = association.AgencyId,
                InsuranceCompanyId = association.InsuranceCompanyId,
                InsuranceCompanyName = association.InsuranceCompanyName,
                InsuranceCompanyCode = association.InsuranceCompanyCode,
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
