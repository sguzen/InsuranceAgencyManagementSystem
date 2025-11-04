using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface ICurrencyRepository : IRepository<Currency>
    {
        Task<Currency?> GetByCodeAsync(string code);
        Task<Currency?> GetBaseCurrencyAsync();
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task<bool> CodeExistsAsync(string code, int? excludeCurrencyId = null);
    }
}
