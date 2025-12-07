using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class CurrencyExchangeRateRepository : Repository<CurrencyExchangeRate>, ICurrencyExchangeRateRepository
    {
        public CurrencyExchangeRateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CurrencyExchangeRate?> GetLatestRateAsync(int fromCurrencyId, int toCurrencyId)
        {
            return await _dbSet
                .Include(r => r.FromCurrency)
                .Include(r => r.ToCurrency)
                .Where(r => !r.IsDeleted &&
                           r.FromCurrencyId == fromCurrencyId &&
                           r.ToCurrencyId == toCurrencyId &&
                           r.EffectiveDate <= DateTime.UtcNow &&
                           (!r.ExpiryDate.HasValue || r.ExpiryDate.Value > DateTime.UtcNow))
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<CurrencyExchangeRate?> GetRateByDateAsync(int fromCurrencyId, int toCurrencyId, DateTime date)
        {
            return await _dbSet
                .Include(r => r.FromCurrency)
                .Include(r => r.ToCurrency)
                .Where(r => !r.IsDeleted &&
                           r.FromCurrencyId == fromCurrencyId &&
                           r.ToCurrencyId == toCurrencyId &&
                           r.EffectiveDate <= date &&
                           (!r.ExpiryDate.HasValue || r.ExpiryDate.Value > date))
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CurrencyExchangeRate>> GetRateHistoryAsync(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(r => r.FromCurrency)
                .Include(r => r.ToCurrency)
                .Where(r => !r.IsDeleted &&
                           r.FromCurrencyId == fromCurrencyId &&
                           r.ToCurrencyId == toCurrencyId &&
                           r.EffectiveDate >= startDate &&
                           r.EffectiveDate <= endDate)
                .OrderByDescending(r => r.EffectiveDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CurrencyExchangeRate>> GetActiveRatesAsync()
        {
            return await _dbSet
                .Include(r => r.FromCurrency)
                .Include(r => r.ToCurrency)
                .Where(r => !r.IsDeleted &&
                           r.EffectiveDate <= DateTime.UtcNow &&
                           (!r.ExpiryDate.HasValue || r.ExpiryDate.Value > DateTime.UtcNow))
                .OrderByDescending(r => r.EffectiveDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetAllRatesToBaseAsync(DateTime? date = null)
        {
            var effectiveDate = date ?? DateTime.UtcNow;

            // Get base currency
            var baseCurrency = await _context.Set<Currency>()
                .Where(c => !c.IsDeleted && c.IsBaseCurrency)
                .FirstOrDefaultAsync();

            if (baseCurrency == null)
            {
                return new Dictionary<string, decimal>();
            }

            // Get all active currencies
            var currencies = await _context.Set<Currency>()
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync();

            var rates = new Dictionary<string, decimal>();

            foreach (var currency in currencies)
            {
                if (currency.Id == baseCurrency.Id)
                {
                    rates[currency.Code] = 1.0m;
                    continue;
                }

                // Try to get rate from currency to base
                var rate = await _dbSet
                    .Where(r => !r.IsDeleted &&
                               r.FromCurrencyId == currency.Id &&
                               r.ToCurrencyId == baseCurrency.Id &&
                               r.EffectiveDate <= effectiveDate &&
                               (!r.ExpiryDate.HasValue || r.ExpiryDate.Value > effectiveDate))
                    .OrderByDescending(r => r.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (rate != null)
                {
                    rates[currency.Code] = rate.Rate;
                }
                else
                {
                    // Try inverse rate (base to currency)
                    var inverseRate = await _dbSet
                        .Where(r => !r.IsDeleted &&
                                   r.FromCurrencyId == baseCurrency.Id &&
                                   r.ToCurrencyId == currency.Id &&
                                   r.EffectiveDate <= effectiveDate &&
                                   (!r.ExpiryDate.HasValue || r.ExpiryDate.Value > effectiveDate))
                        .OrderByDescending(r => r.EffectiveDate)
                        .FirstOrDefaultAsync();

                    if (inverseRate != null && inverseRate.Rate != 0)
                    {
                        rates[currency.Code] = 1 / inverseRate.Rate;
                    }
                }
            }

            return rates;
        }
    }
}
