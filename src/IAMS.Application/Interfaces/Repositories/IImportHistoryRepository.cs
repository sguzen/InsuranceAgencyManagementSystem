using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IImportHistoryRepository : IRepository<ImportHistory>
    {
        Task<List<ImportHistory>> GetRecentImportsAsync(int count = 10);
        Task<List<ImportHistory>> GetByInsuranceCompanyIdAsync(int insuranceCompanyId);
        Task<List<ImportHistory>> GetBySourceTypeAsync(ImportSourceType sourceType);
        Task<List<ImportHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ImportHistory?> GetLastSuccessfulSyncAsync(int configurationId);
        Task<List<ImportHistory>> GetFailedImportsAsync();
    }
}
