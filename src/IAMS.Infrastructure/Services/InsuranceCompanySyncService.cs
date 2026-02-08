using IAMS.MultiTenancy.Data;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Synchronizes insurance companies from master database to tenant database.
    /// When an agency is linked to an insurance company, ensures the tenant DB has a matching record.
    /// </summary>
    public interface IInsuranceCompanySyncService
    {
        /// <summary>
        /// Syncs all insurance companies linked to the current tenant/agency.
        /// Creates missing InsuranceCompany records in the tenant database.
        /// </summary>
        Task<int> SyncInsuranceCompaniesForTenantAsync(int agencyId, string createdBy = "System");

        /// <summary>
        /// Syncs a specific insurance company to the tenant database.
        /// </summary>
        Task<bool> SyncInsuranceCompanyAsync(int agencyId, int masterInsuranceCompanyId, string createdBy = "System");

        /// <summary>
        /// Gets the tenant InsuranceCompany ID for a master InsuranceCompany ID.
        /// Creates the tenant record if it doesn't exist.
        /// </summary>
        Task<int> GetOrCreateTenantInsuranceCompanyIdAsync(int agencyId, int masterInsuranceCompanyId, string createdBy = "System");
    }

    public class InsuranceCompanySyncService : IInsuranceCompanySyncService
    {
        private readonly TenantDbContext _masterDb;
        private readonly ApplicationDbContext _tenantDb;
        private readonly ILogger<InsuranceCompanySyncService> _logger;

        public InsuranceCompanySyncService(
            TenantDbContext masterDb,
            ApplicationDbContext tenantDb,
            ILogger<InsuranceCompanySyncService> logger)
        {
            _masterDb = masterDb;
            _tenantDb = tenantDb;
            _logger = logger;
        }

        public async Task<int> SyncInsuranceCompaniesForTenantAsync(int agencyId, string createdBy = "System")
        {
            // Get all insurance companies linked to this agency
            var linkedCompanies = await _masterDb.AgencyInsuranceCompanies
                .AsNoTracking()
                .Where(aic => aic.AgencyId == agencyId && !aic.IsDeleted && aic.IsActive)
                .Select(aic => aic.InsuranceCompanyId)
                .ToListAsync();

            if (!linkedCompanies.Any())
            {
                _logger.LogInformation("No insurance companies linked to agency {AgencyId}", agencyId);
                return 0;
            }

            // Get master company details
            var masterCompanies = await _masterDb.InsuranceCompanies
                .AsNoTracking()
                .Where(ic => linkedCompanies.Contains(ic.Id) && ic.IsActive)
                .ToListAsync();

            // Get existing tenant companies by MasterInsuranceCompanyId
            var existingMasterIds = await _tenantDb.InsuranceCompanies
                .AsNoTracking()
                .Where(ic => ic.MasterInsuranceCompanyId.HasValue)
                .Select(ic => ic.MasterInsuranceCompanyId!.Value)
                .ToListAsync();

            int syncedCount = 0;

            foreach (var masterCompany in masterCompanies)
            {
                if (existingMasterIds.Contains(masterCompany.Id))
                {
                    // Already exists, update if needed
                    var tenantCompany = await _tenantDb.InsuranceCompanies
                        .FirstAsync(ic => ic.MasterInsuranceCompanyId == masterCompany.Id);

                    if (tenantCompany.Name != masterCompany.Name || tenantCompany.Code != masterCompany.Code)
                    {
                        tenantCompany.Name = masterCompany.Name;
                        tenantCompany.Code = masterCompany.Code;
                        tenantCompany.LogoUrl = masterCompany.LogoUrl;
                        tenantCompany.Website = masterCompany.Website;
                        tenantCompany.ModifiedBy = createdBy;
                        tenantCompany.ModifiedOn = DateTime.UtcNow;
                        syncedCount++;
                    }
                }
                else
                {
                    // Create new tenant company
                    var tenantCompany = new Domain.Entities.InsuranceCompany
                    {
                        Name = masterCompany.Name,
                        Code = masterCompany.Code,
                        LogoUrl = masterCompany.LogoUrl,
                        Website = masterCompany.Website,
                        MasterInsuranceCompanyId = masterCompany.Id,
                        IsActive = true,
                        CreatedBy = createdBy,
                        CreatedOn = DateTime.UtcNow
                    };

                    _tenantDb.InsuranceCompanies.Add(tenantCompany);
                    syncedCount++;

                    _logger.LogInformation("Created tenant InsuranceCompany {Name} linked to master {MasterId}",
                        masterCompany.Name, masterCompany.Id);
                }
            }

            if (syncedCount > 0)
            {
                await _tenantDb.SaveChangesAsync();
                _logger.LogInformation("Synced {Count} insurance companies for agency {AgencyId}",
                    syncedCount, agencyId);
            }

            return syncedCount;
        }

        public async Task<bool> SyncInsuranceCompanyAsync(int agencyId, int masterInsuranceCompanyId, string createdBy = "System")
        {
            // Verify the agency is linked to this insurance company
            var isLinked = await _masterDb.AgencyInsuranceCompanies
                .AnyAsync(aic => aic.AgencyId == agencyId
                    && aic.InsuranceCompanyId == masterInsuranceCompanyId
                    && !aic.IsDeleted
                    && aic.IsActive);

            if (!isLinked)
            {
                _logger.LogWarning("Agency {AgencyId} is not linked to InsuranceCompany {MasterId}",
                    agencyId, masterInsuranceCompanyId);
                return false;
            }

            await GetOrCreateTenantInsuranceCompanyIdAsync(agencyId, masterInsuranceCompanyId, createdBy);
            return true;
        }

        public async Task<int> GetOrCreateTenantInsuranceCompanyIdAsync(int agencyId, int masterInsuranceCompanyId, string createdBy = "System")
        {
            // Check if tenant already has this company
            var existingTenantCompany = await _tenantDb.InsuranceCompanies
                .FirstOrDefaultAsync(ic => ic.MasterInsuranceCompanyId == masterInsuranceCompanyId);

            if (existingTenantCompany != null)
            {
                return existingTenantCompany.Id;
            }

            // Get master company details
            var masterCompany = await _masterDb.InsuranceCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ic => ic.Id == masterInsuranceCompanyId);

            if (masterCompany == null)
            {
                throw new InvalidOperationException($"Master InsuranceCompany {masterInsuranceCompanyId} not found");
            }

            // Create tenant company
            var tenantCompany = new Domain.Entities.InsuranceCompany
            {
                Name = masterCompany.Name,
                Code = masterCompany.Code,
                LogoUrl = masterCompany.LogoUrl,
                Website = masterCompany.Website,
                MasterInsuranceCompanyId = masterCompany.Id,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow
            };

            _tenantDb.InsuranceCompanies.Add(tenantCompany);
            await _tenantDb.SaveChangesAsync();

            _logger.LogInformation("Created tenant InsuranceCompany {Name} (ID: {Id}) linked to master {MasterId}",
                tenantCompany.Name, tenantCompany.Id, masterCompany.Id);

            return tenantCompany.Id;
        }
    }
}
