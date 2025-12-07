using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class ImportHistoryRepository : Repository<ImportHistory>, IImportHistoryRepository
    {
        public ImportHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ImportHistory>> GetRecentImportsAsync(int count = 10)
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ImportHistory>> GetByInsuranceCompanyIdAsync(int insuranceCompanyId)
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => ih.InsuranceCompanyId == insuranceCompanyId && !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .ToListAsync();
        }

        public async Task<List<ImportHistory>> GetBySourceTypeAsync(ImportSourceType sourceType)
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => ih.SourceType == sourceType && !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .ToListAsync();
        }

        public async Task<List<ImportHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => ih.StartedAt >= startDate && ih.StartedAt <= endDate && !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .ToListAsync();
        }

        public async Task<ImportHistory?> GetLastSuccessfulSyncAsync(int configurationId)
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => ih.ImportConfigurationId == configurationId
                    && ih.Status == "Success"
                    && !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ImportHistory>> GetFailedImportsAsync()
        {
            return await _context.ImportHistories
                .Include(ih => ih.ImportConfiguration)
                .Include(ih => ih.InsuranceCompany)
                .Where(ih => ih.Status == "Failed" && !ih.IsDeleted)
                .OrderByDescending(ih => ih.StartedAt)
                .ToListAsync();
        }
    }
}
