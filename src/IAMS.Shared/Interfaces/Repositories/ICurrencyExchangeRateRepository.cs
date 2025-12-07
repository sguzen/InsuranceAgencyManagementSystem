using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface ICurrencyExchangeRateRepository : IRepository<CurrencyExchangeRate>
    {
        Task<CurrencyExchangeRate?> GetLatestRateAsync(int fromCurrencyId, int toCurrencyId);
        Task<CurrencyExchangeRate?> GetRateByDateAsync(int fromCurrencyId, int toCurrencyId, DateTime date);
        Task<IEnumerable<CurrencyExchangeRate>> GetRateHistoryAsync(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CurrencyExchangeRate>> GetActiveRatesAsync();
        Task<Dictionary<string, decimal>> GetAllRatesToBaseAsync(DateTime? date = null);
    }
}
