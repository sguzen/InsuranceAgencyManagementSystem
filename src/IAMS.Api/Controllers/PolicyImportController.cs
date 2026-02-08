using IAMS.Domain.Entities;
using IAMS.Infrastructure.Services;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Contexts;
using IAMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Api.Controllers
{
    /// <summary>
    /// Handles policy import requests from Web app.
    /// Web app requests import → API queues job → Background worker processes with credentials.
    /// Credentials are NEVER exposed to the Web app.
    /// </summary>
    [ApiController]
    [Route("api/import")]
    [Authorize(Policy = "ApiKeyOrJwt")]
    public class PolicyImportController : ControllerBase
    {
        private readonly ApplicationDbContext _tenantDb;
        private readonly TenantDbContext _masterDb;
        private readonly ITenantContextAccessor _tenantContext;
        private readonly IInsuranceCompanySyncService _syncService;
        private readonly ILogger<PolicyImportController> _logger;

        public PolicyImportController(
            ApplicationDbContext tenantDb,
            TenantDbContext masterDb,
            ITenantContextAccessor tenantContext,
            IInsuranceCompanySyncService syncService,
            ILogger<PolicyImportController> logger)
        {
            _tenantDb = tenantDb;
            _masterDb = masterDb;
            _tenantContext = tenantContext;
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Get insurance companies available for import (linked to this agency).
        /// </summary>
        [HttpGet("available-companies")]
        public async Task<ActionResult<Result<List<ImportableCompanyDto>>>> GetAvailableCompanies()
        {
            try
            {
                var agencyId = _tenantContext.TenantId;
                if (agencyId == null)
                {
                    return BadRequest(Result<List<ImportableCompanyDto>>.Failure("Tenant not identified"));
                }

                // Get companies linked to this agency with active credentials
                var linkedCompanies = await _masterDb.AgencyInsuranceCompanies
                    .AsNoTracking()
                    .Where(aic => aic.AgencyId == agencyId.Value && !aic.IsDeleted && aic.IsActive)
                    .Include(aic => aic.InsuranceCompany)
                    .Select(aic => new ImportableCompanyDto
                    {
                        MasterInsuranceCompanyId = aic.InsuranceCompanyId,
                        Name = aic.InsuranceCompany.Name,
                        Code = aic.InsuranceCompany.Code,
                        HasCredentials = !string.IsNullOrEmpty(aic.DbServer) || !string.IsNullOrEmpty(aic.ConnectionString),
                        LastImportDate = null // Will be populated below
                    })
                    .ToListAsync();

                // Get last import dates from tenant DB
                var masterIds = linkedCompanies.Select(c => c.MasterInsuranceCompanyId).ToList();
                var lastImports = await _tenantDb.ImportJobs
                    .Where(j => masterIds.Contains(j.MasterInsuranceCompanyId) && j.Status == ImportJobStatus.Completed)
                    .GroupBy(j => j.MasterInsuranceCompanyId)
                    .Select(g => new { MasterInsuranceCompanyId = g.Key, LastImport = g.Max(j => j.CompletedAt) })
                    .ToListAsync();

                foreach (var company in linkedCompanies)
                {
                    company.LastImportDate = lastImports
                        .FirstOrDefault(i => i.MasterInsuranceCompanyId == company.MasterInsuranceCompanyId)
                        ?.LastImport;
                }

                return Ok(Result<List<ImportableCompanyDto>>.Success(linkedCompanies));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available companies for import");
                return StatusCode(500, Result<List<ImportableCompanyDto>>.Failure("Error retrieving companies"));
            }
        }

        /// <summary>
        /// Request a policy import from an insurance company.
        /// Creates a job that will be processed by the background worker.
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult<Result<ImportJobDto>>> RequestImport([FromBody] ImportRequestDto request)
        {
            try
            {
                var agencyId = _tenantContext.TenantId;
                if (agencyId == null)
                {
                    return BadRequest(Result<ImportJobDto>.Failure("Tenant not identified"));
                }

                // Verify the agency is linked to this insurance company
                var link = await _masterDb.AgencyInsuranceCompanies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(aic =>
                        aic.AgencyId == agencyId.Value &&
                        aic.InsuranceCompanyId == request.MasterInsuranceCompanyId &&
                        !aic.IsDeleted &&
                        aic.IsActive);

                if (link == null)
                {
                    return BadRequest(Result<ImportJobDto>.Failure("Bu sigorta şirketine erişim izniniz yok"));
                }

                // Check for existing pending/running jobs
                var existingJob = await _tenantDb.ImportJobs
                    .AnyAsync(j => j.MasterInsuranceCompanyId == request.MasterInsuranceCompanyId &&
                        (j.Status == ImportJobStatus.Pending || j.Status == ImportJobStatus.Running));

                if (existingJob)
                {
                    return BadRequest(Result<ImportJobDto>.Failure("Bu şirket için zaten devam eden bir import işlemi var"));
                }

                // Ensure tenant has the insurance company record
                var tenantCompanyId = await _syncService.GetOrCreateTenantInsuranceCompanyIdAsync(
                    agencyId.Value, request.MasterInsuranceCompanyId, User.Identity?.Name ?? "System");

                // Create import job
                var job = new ImportJob
                {
                    AgencyId = agencyId.Value,
                    InsuranceCompanyId = tenantCompanyId,
                    MasterInsuranceCompanyId = request.MasterInsuranceCompanyId,
                    FilterStartDate = request.StartDate,
                    FilterEndDate = request.EndDate,
                    RequestedBy = User.Identity?.Name ?? "Unknown",
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedOn = DateTime.UtcNow
                };

                _tenantDb.ImportJobs.Add(job);
                await _tenantDb.SaveChangesAsync();

                _logger.LogInformation("Import job {JobId} created for agency {AgencyId}, company {CompanyId}",
                    job.Id, agencyId.Value, request.MasterInsuranceCompanyId);

                return Ok(Result<ImportJobDto>.Success(MapToDto(job), "Import işlemi kuyruğa alındı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating import request");
                return StatusCode(500, Result<ImportJobDto>.Failure("Import işlemi oluşturulurken hata oluştu"));
            }
        }

        /// <summary>
        /// Get import job status.
        /// </summary>
        [HttpGet("jobs/{jobId}")]
        public async Task<ActionResult<Result<ImportJobDto>>> GetJobStatus(int jobId)
        {
            try
            {
                var job = await _tenantDb.ImportJobs
                    .AsNoTracking()
                    .Include(j => j.InsuranceCompany)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return NotFound(Result<ImportJobDto>.Failure("Import işlemi bulunamadı"));
                }

                return Ok(Result<ImportJobDto>.Success(MapToDto(job)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting import job status");
                return StatusCode(500, Result<ImportJobDto>.Failure("Error retrieving job status"));
            }
        }

        /// <summary>
        /// Get recent import jobs.
        /// </summary>
        [HttpGet("jobs")]
        public async Task<ActionResult<Result<List<ImportJobDto>>>> GetRecentJobs([FromQuery] int? insuranceCompanyId = null)
        {
            try
            {
                var query = _tenantDb.ImportJobs
                    .AsNoTracking()
                    .Include(j => j.InsuranceCompany)
                    .AsQueryable();

                if (insuranceCompanyId.HasValue)
                {
                    query = query.Where(j => j.InsuranceCompanyId == insuranceCompanyId.Value);
                }

                var jobs = await query
                    .OrderByDescending(j => j.CreatedOn)
                    .Take(20)
                    .Select(j => MapToDto(j))
                    .ToListAsync();

                return Ok(Result<List<ImportJobDto>>.Success(jobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting import jobs");
                return StatusCode(500, Result<List<ImportJobDto>>.Failure("Error retrieving jobs"));
            }
        }

        /// <summary>
        /// Cancel a pending import job.
        /// </summary>
        [HttpPost("jobs/{jobId}/cancel")]
        public async Task<ActionResult<Result>> CancelJob(int jobId)
        {
            try
            {
                var job = await _tenantDb.ImportJobs.FindAsync(jobId);

                if (job == null)
                {
                    return NotFound(Result.Failure("Import işlemi bulunamadı"));
                }

                if (job.Status != ImportJobStatus.Pending)
                {
                    return BadRequest(Result.Failure("Sadece bekleyen işlemler iptal edilebilir"));
                }

                job.Cancel("Kullanıcı tarafından iptal edildi");
                await _tenantDb.SaveChangesAsync();

                return Ok(Result.Success("Import işlemi iptal edildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling import job");
                return StatusCode(500, Result.Failure("Error cancelling job"));
            }
        }

        private static ImportJobDto MapToDto(ImportJob job)
        {
            return new ImportJobDto
            {
                Id = job.Id,
                InsuranceCompanyId = job.InsuranceCompanyId,
                InsuranceCompanyName = job.InsuranceCompany?.Name,
                Status = job.Status.ToString(),
                CreatedOn = job.CreatedOn,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                TotalRecords = job.TotalRecords,
                ImportedRecords = job.ImportedRecords,
                FailedRecords = job.FailedRecords,
                SkippedRecords = job.SkippedRecords,
                ErrorMessage = job.ErrorMessage,
                RequestedBy = job.RequestedBy,
                Duration = job.Duration?.TotalSeconds
            };
        }
    }

    // DTOs
    public class ImportableCompanyDto
    {
        public int MasterInsuranceCompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool HasCredentials { get; set; }
        public DateTime? LastImportDate { get; set; }
    }

    public class ImportRequestDto
    {
        public int MasterInsuranceCompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ImportJobDto
    {
        public int Id { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string? InsuranceCompanyName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public double? Duration { get; set; }
    }
}
