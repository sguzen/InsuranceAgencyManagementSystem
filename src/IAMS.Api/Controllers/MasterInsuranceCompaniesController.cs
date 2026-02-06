using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.Shared.DTOs.MasterData;
using IAMS.Shared.Models;

namespace IAMS.Api.Controllers
{
    /// <summary>
    /// Manages the master list of insurance companies (system-wide).
    /// For Admin panel use only.
    /// </summary>
    [ApiController]
    [Route("api/master/insurance-companies")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MasterInsuranceCompaniesController : ControllerBase
    {
        private readonly TenantDbContext _tenantDb;
        private readonly ILogger<MasterInsuranceCompaniesController> _logger;

        public MasterInsuranceCompaniesController(
            TenantDbContext tenantDb,
            ILogger<MasterInsuranceCompaniesController> logger)
        {
            _tenantDb = tenantDb;
            _logger = logger;
        }

        /// <summary>
        /// Get all insurance companies with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<List<MasterInsuranceCompanyDto>>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _tenantDb.InsuranceCompanies
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(ic =>
                        ic.Name.ToLower().Contains(search) ||
                        ic.Code.ToLower().Contains(search));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(ic => ic.IsActive == isActive.Value);
                }

                var companies = await query
                    .OrderBy(ic => ic.DisplayOrder)
                    .ThenBy(ic => ic.Name)
                    .Select(ic => new MasterInsuranceCompanyDto
                    {
                        Id = ic.Id,
                        Name = ic.Name,
                        Code = ic.Code,
                        LogoUrl = ic.LogoUrl,
                        Website = ic.Website,
                        ApiEndpoint = ic.ApiEndpoint,
                        IntegrationSettings = ic.IntegrationSettings,
                        IsActive = ic.IsActive,
                        DisplayOrder = ic.DisplayOrder,
                        CreatedOn = ic.CreatedOn,
                        ModifiedOn = ic.ModifiedOn,
                        CreatedBy = ic.CreatedBy,
                        ModifiedBy = ic.ModifiedBy,
                        LinkedAgenciesCount = ic.AgencyInsuranceCompanies.Count(aic => !aic.IsDeleted)
                    })
                    .ToListAsync();

                return Ok(Result<List<MasterInsuranceCompanyDto>>.Success(companies));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance companies");
                return StatusCode(500, Result<List<MasterInsuranceCompanyDto>>.Failure("Error retrieving insurance companies"));
            }
        }

        /// <summary>
        /// Get insurance companies for dropdown selection
        /// </summary>
        [HttpGet("select")]
        public async Task<ActionResult<Result<List<InsuranceCompanySelectDto>>>> GetForSelect()
        {
            try
            {
                var companies = await _tenantDb.InsuranceCompanies
                    .AsNoTracking()
                    .Where(ic => ic.IsActive)
                    .OrderBy(ic => ic.DisplayOrder)
                    .ThenBy(ic => ic.Name)
                    .Select(ic => new InsuranceCompanySelectDto
                    {
                        Id = ic.Id,
                        Name = ic.Name,
                        Code = ic.Code
                    })
                    .ToListAsync();

                return Ok(Result<List<InsuranceCompanySelectDto>>.Success(companies));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance companies for select");
                return StatusCode(500, Result<List<InsuranceCompanySelectDto>>.Failure("Error retrieving insurance companies"));
            }
        }

        /// <summary>
        /// Get a single insurance company by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<MasterInsuranceCompanyDto>>> GetById(int id)
        {
            try
            {
                var company = await _tenantDb.InsuranceCompanies
                    .AsNoTracking()
                    .Where(ic => ic.Id == id)
                    .Select(ic => new MasterInsuranceCompanyDto
                    {
                        Id = ic.Id,
                        Name = ic.Name,
                        Code = ic.Code,
                        LogoUrl = ic.LogoUrl,
                        Website = ic.Website,
                        ApiEndpoint = ic.ApiEndpoint,
                        IntegrationSettings = ic.IntegrationSettings,
                        IsActive = ic.IsActive,
                        DisplayOrder = ic.DisplayOrder,
                        CreatedOn = ic.CreatedOn,
                        ModifiedOn = ic.ModifiedOn,
                        CreatedBy = ic.CreatedBy,
                        ModifiedBy = ic.ModifiedBy,
                        LinkedAgenciesCount = ic.AgencyInsuranceCompanies.Count(aic => !aic.IsDeleted)
                    })
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound(Result<MasterInsuranceCompanyDto>.Failure("Sigorta şirketi bulunamadı"));
                }

                return Ok(Result<MasterInsuranceCompanyDto>.Success(company));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance company {Id}", id);
                return StatusCode(500, Result<MasterInsuranceCompanyDto>.Failure("Error retrieving insurance company"));
            }
        }

        /// <summary>
        /// Create a new insurance company
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<MasterInsuranceCompanyDto>>> Create([FromBody] CreateMasterInsuranceCompanyDto dto)
        {
            try
            {
                // Check for duplicate code
                var existingCode = await _tenantDb.InsuranceCompanies
                    .AnyAsync(ic => ic.Code == dto.Code);

                if (existingCode)
                {
                    return BadRequest(Result<MasterInsuranceCompanyDto>.Failure($"'{dto.Code}' kodu zaten kullanılıyor"));
                }

                var company = new InsuranceCompany
                {
                    Name = dto.Name,
                    Code = dto.Code.ToUpperInvariant(),
                    LogoUrl = dto.LogoUrl,
                    Website = dto.Website,
                    ApiEndpoint = dto.ApiEndpoint,
                    IntegrationSettings = dto.IntegrationSettings,
                    IsActive = dto.IsActive,
                    DisplayOrder = dto.DisplayOrder,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name
                };

                _tenantDb.InsuranceCompanies.Add(company);
                await _tenantDb.SaveChangesAsync();

                var result = new MasterInsuranceCompanyDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Code = company.Code,
                    LogoUrl = company.LogoUrl,
                    Website = company.Website,
                    ApiEndpoint = company.ApiEndpoint,
                    IntegrationSettings = company.IntegrationSettings,
                    IsActive = company.IsActive,
                    DisplayOrder = company.DisplayOrder,
                    CreatedOn = company.CreatedOn,
                    CreatedBy = company.CreatedBy,
                    LinkedAgenciesCount = 0
                };

                _logger.LogInformation("Created insurance company {Id}: {Name}", company.Id, company.Name);
                return Ok(Result<MasterInsuranceCompanyDto>.Success(result, "Sigorta şirketi başarıyla oluşturuldu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance company");
                return StatusCode(500, Result<MasterInsuranceCompanyDto>.Failure("Error creating insurance company"));
            }
        }

        /// <summary>
        /// Update an insurance company
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Result>> Update(int id, [FromBody] UpdateMasterInsuranceCompanyDto dto)
        {
            try
            {
                var company = await _tenantDb.InsuranceCompanies
                    .FirstOrDefaultAsync(ic => ic.Id == id);

                if (company == null)
                {
                    return NotFound(Result.Failure("Sigorta şirketi bulunamadı"));
                }

                // Check for duplicate code (excluding current)
                var existingCode = await _tenantDb.InsuranceCompanies
                    .AnyAsync(ic => ic.Code == dto.Code && ic.Id != id);

                if (existingCode)
                {
                    return BadRequest(Result.Failure($"'{dto.Code}' kodu zaten kullanılıyor"));
                }

                company.Name = dto.Name;
                company.Code = dto.Code.ToUpperInvariant();
                company.LogoUrl = dto.LogoUrl;
                company.Website = dto.Website;
                company.ApiEndpoint = dto.ApiEndpoint;
                company.IntegrationSettings = dto.IntegrationSettings;
                company.IsActive = dto.IsActive;
                company.DisplayOrder = dto.DisplayOrder;
                company.ModifiedOn = DateTime.UtcNow;
                company.ModifiedBy = User.Identity?.Name;

                await _tenantDb.SaveChangesAsync();

                _logger.LogInformation("Updated insurance company {Id}: {Name}", company.Id, company.Name);
                return Ok(Result.Success("Sigorta şirketi başarıyla güncellendi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance company {Id}", id);
                return StatusCode(500, Result.Failure("Error updating insurance company"));
            }
        }

        /// <summary>
        /// Delete an insurance company
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Result>> Delete(int id)
        {
            try
            {
                var company = await _tenantDb.InsuranceCompanies
                    .Include(ic => ic.AgencyInsuranceCompanies)
                    .FirstOrDefaultAsync(ic => ic.Id == id);

                if (company == null)
                {
                    return NotFound(Result.Failure("Sigorta şirketi bulunamadı"));
                }

                // Check if linked to any agencies
                var linkedCount = company.AgencyInsuranceCompanies.Count(aic => !aic.IsDeleted);
                if (linkedCount > 0)
                {
                    return BadRequest(Result.Failure($"Bu sigorta şirketi {linkedCount} acentaya bağlı. Önce bağlantıları kaldırın."));
                }

                _tenantDb.InsuranceCompanies.Remove(company);
                await _tenantDb.SaveChangesAsync();

                _logger.LogInformation("Deleted insurance company {Id}: {Name}", company.Id, company.Name);
                return Ok(Result.Success("Sigorta şirketi başarıyla silindi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insurance company {Id}", id);
                return StatusCode(500, Result.Failure("Error deleting insurance company"));
            }
        }

        /// <summary>
        /// Toggle active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        public async Task<ActionResult<Result>> ToggleStatus(int id)
        {
            try
            {
                var company = await _tenantDb.InsuranceCompanies
                    .FirstOrDefaultAsync(ic => ic.Id == id);

                if (company == null)
                {
                    return NotFound(Result.Failure("Sigorta şirketi bulunamadı"));
                }

                company.IsActive = !company.IsActive;
                company.ModifiedOn = DateTime.UtcNow;
                company.ModifiedBy = User.Identity?.Name;

                await _tenantDb.SaveChangesAsync();

                var status = company.IsActive ? "aktif" : "pasif";
                _logger.LogInformation("Toggled insurance company {Id} status to {Status}", company.Id, status);
                return Ok(Result.Success($"Sigorta şirketi {status} durumuna getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling insurance company {Id} status", id);
                return StatusCode(500, Result.Failure("Error toggling status"));
            }
        }
    }
}
