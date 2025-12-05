using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IImportConfigurationRepository : IRepository<ImportConfiguration>
    {
        Task<List<ImportConfiguration>> GetActiveConfigurationsAsync();
        Task<List<ImportConfiguration>> GetByInsuranceCompanyIdAsync(int insuranceCompanyId);
        Task<List<ImportConfiguration>> GetBySourceTypeAsync(ImportSourceType sourceType);
        Task<ImportConfiguration?> GetByNameAsync(string name);
        Task<List<ImportConfiguration>> GetAutoSyncEnabledAsync();
    }
}
