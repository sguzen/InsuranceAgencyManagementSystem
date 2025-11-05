using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Code == code)
                .FirstOrDefaultAsync();
        }

        public async Task<Currency?> GetBaseCurrencyAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IsBaseCurrency)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeCurrencyId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Code == code);

            if (excludeCurrencyId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCurrencyId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
