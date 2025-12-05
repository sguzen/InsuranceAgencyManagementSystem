using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class ImportConfigurationRepository : Repository<ImportConfiguration>, IImportConfigurationRepository
    {
        public ImportConfigurationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ImportConfiguration>> GetActiveConfigurationsAsync()
        {
            return await _context.ImportConfigurations
                .Include(ic => ic.InsuranceCompany)
                .Where(ic => ic.IsActive && !ic.IsDeleted)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<List<ImportConfiguration>> GetByInsuranceCompanyIdAsync(int insuranceCompanyId)
        {
            return await _context.ImportConfigurations
                .Include(ic => ic.InsuranceCompany)
                .Where(ic => ic.InsuranceCompanyId == insuranceCompanyId && !ic.IsDeleted)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<List<ImportConfiguration>> GetBySourceTypeAsync(ImportSourceType sourceType)
        {
            return await _context.ImportConfigurations
                .Include(ic => ic.InsuranceCompany)
                .Where(ic => ic.SourceType == sourceType && !ic.IsDeleted)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<ImportConfiguration?> GetByNameAsync(string name)
        {
            return await _context.ImportConfigurations
                .Include(ic => ic.InsuranceCompany)
                .FirstOrDefaultAsync(ic => ic.Name == name && !ic.IsDeleted);
        }

        public async Task<List<ImportConfiguration>> GetAutoSyncEnabledAsync()
        {
            return await _context.ImportConfigurations
                .Include(ic => ic.InsuranceCompany)
                .Where(ic => ic.EnableAutoSync && ic.IsActive && !ic.IsDeleted)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }
    }
}
