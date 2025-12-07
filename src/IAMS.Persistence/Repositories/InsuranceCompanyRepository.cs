using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class InsuranceCompanyRepository : Repository<InsuranceCompany>, IInsuranceCompanyRepository
    {
        public InsuranceCompanyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<InsuranceCompany>> GetActiveCompaniesAsync()
        {
            return await _dbSet
                .Where(ic => ic.IsActive && !ic.IsDeleted)
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<InsuranceCompany?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Where(ic => !ic.IsDeleted)
                .FirstOrDefaultAsync(ic => ic.Code == code);
        }

        public async Task<InsuranceCompany?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(ic => !ic.IsDeleted)
                .FirstOrDefaultAsync(ic => ic.Name == name);
        }

        public async Task<IEnumerable<InsuranceCompany>> GetCompaniesWithIntegrationAsync()
        {
            return await _dbSet
                .Where(ic => ic.IsActive && !ic.IsDeleted &&
                            !string.IsNullOrEmpty(ic.ApiEndpoint))
                .OrderBy(ic => ic.Name)
                .ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.Where(ic => !ic.IsDeleted && ic.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(ic => ic.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(ic => !ic.IsDeleted && ic.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(ic => ic.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<CommissionRate>> GetCommissionRatesAsync(int companyId)
        {
            var company = await _dbSet
                .Include(ic => ic.CommissionRates.Where(cr => cr.IsActive && !cr.IsDeleted))
                    .ThenInclude(cr => cr.PolicyType)
                .Where(ic => ic.Id == companyId && !ic.IsDeleted)
                .FirstOrDefaultAsync();

            return company?.CommissionRates ?? new List<CommissionRate>();
        }

        public async Task<int> GetActiveCustomerCountAsync(int companyId)
        {
            return await _context.CustomerInsuranceCompanies
                .Where(cic => cic.InsuranceCompanyId == companyId &&
                             cic.IsActive && !cic.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalPremiumAmountAsync(int companyId)
        {
            // Get the base currency (TRY)
            var baseCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsBaseCurrency && !c.IsDeleted);

            if (baseCurrency == null)
            {
                // Fallback: sum without conversion if base currency not found
                return await _context.Policies
                    .Where(p => p.InsuranceCompanyId == companyId &&
                               p.Status == PolicyStatus.Active && !p.IsDeleted)
                    .SumAsync(p => p.PremiumAmount);
            }

            // Get all active policies with their currencies
            var policies = await _context.Policies
                .Where(p => p.InsuranceCompanyId == companyId &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .Select(p => new
                {
                    p.PremiumAmount,
                    p.PremiumAmountInBaseCurrency,
                    p.ExchangeRateToBase,
                    p.CurrencyId
                })
                .ToListAsync();

            // Get current exchange rates to base currency for all distinct currencies
            var currencyIds = policies.Select(p => p.CurrencyId).Distinct().ToList();
            var today = DateTime.Today;

            var exchangeRates = await _context.CurrencyExchangeRates
                .Where(er => currencyIds.Contains(er.FromCurrencyId) &&
                            er.ToCurrencyId == baseCurrency.Id &&
                            er.EffectiveDate <= today &&
                            (!er.ExpiryDate.HasValue || er.ExpiryDate.Value > today))
                .GroupBy(er => er.FromCurrencyId)
                .Select(g => new
                {
                    CurrencyId = g.Key,
                    Rate = g.OrderByDescending(er => er.EffectiveDate).First().Rate
                })
                .ToDictionaryAsync(x => x.CurrencyId, x => x.Rate);

            // Calculate total in base currency
            decimal total = 0;
            foreach (var policy in policies)
            {
                if (policy.PremiumAmountInBaseCurrency.HasValue)
                {
                    // Use pre-calculated amount
                    total += policy.PremiumAmountInBaseCurrency.Value;
                }
                else if (policy.ExchangeRateToBase.HasValue)
                {
                    // Use stored exchange rate
                    total += policy.PremiumAmount * policy.ExchangeRateToBase.Value;
                }
                else if (policy.CurrencyId == baseCurrency.Id)
                {
                    // Already in base currency
                    total += policy.PremiumAmount;
                }
                else if (exchangeRates.TryGetValue(policy.CurrencyId, out var rate))
                {
                    // Use current exchange rate
                    total += policy.PremiumAmount * rate;
                }
                else
                {
                    // No exchange rate available - use 1:1 (not ideal but better than skipping)
                    total += policy.PremiumAmount;
                }
            }

            return total;
        }

        public async Task<int> GetActivePoliciesCountAsync(int id)
        {
            return await _context.Policies
                .Where(p => p.InsuranceCompanyId == id &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalCommissionsAsync(int id)
        {
            // Get the base currency (TRY)
            var baseCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsBaseCurrency && !c.IsDeleted);

            if (baseCurrency == null)
            {
                // Fallback: sum without conversion if base currency not found
                return await _context.Policies
                    .Where(p => p.InsuranceCompanyId == id &&
                               p.Status == PolicyStatus.Active && !p.IsDeleted)
                    .SumAsync(p => p.CommissionAmount);
            }

            // Get all active policies with their currencies
            var policies = await _context.Policies
                .Where(p => p.InsuranceCompanyId == id &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .Select(p => new
                {
                    p.CommissionAmount,
                    p.ExchangeRateToBase,
                    p.CurrencyId
                })
                .ToListAsync();

            // Get current exchange rates to base currency for all distinct currencies
            var currencyIds = policies.Select(p => p.CurrencyId).Distinct().ToList();
            var today = DateTime.Today;

            var exchangeRates = await _context.CurrencyExchangeRates
                .Where(er => currencyIds.Contains(er.FromCurrencyId) &&
                            er.ToCurrencyId == baseCurrency.Id &&
                            er.EffectiveDate <= today &&
                            (!er.ExpiryDate.HasValue || er.ExpiryDate.Value > today))
                .GroupBy(er => er.FromCurrencyId)
                .Select(g => new
                {
                    CurrencyId = g.Key,
                    Rate = g.OrderByDescending(er => er.EffectiveDate).First().Rate
                })
                .ToDictionaryAsync(x => x.CurrencyId, x => x.Rate);

            // Calculate total in base currency
            decimal total = 0;
            foreach (var policy in policies)
            {
                if (policy.ExchangeRateToBase.HasValue)
                {
                    // Use stored exchange rate
                    total += policy.CommissionAmount * policy.ExchangeRateToBase.Value;
                }
                else if (policy.CurrencyId == baseCurrency.Id)
                {
                    // Already in base currency
                    total += policy.CommissionAmount;
                }
                else if (exchangeRates.TryGetValue(policy.CurrencyId, out var rate))
                {
                    // Use current exchange rate
                    total += policy.CommissionAmount * rate;
                }
                else
                {
                    // No exchange rate available - use 1:1 (not ideal but better than skipping)
                    total += policy.CommissionAmount;
                }
            }

            return total;
        }

        public async Task<List<CurrencyBreakdownDto>> GetCurrencyBreakdownAsync(int companyId)
        {
            return await _context.Policies
                .Where(p => p.InsuranceCompanyId == companyId &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .GroupBy(p => new { p.CurrencyId, p.Currency.Code, p.Currency.Symbol })
                .Select(g => new CurrencyBreakdownDto
                {
                    CurrencyId = g.Key.CurrencyId,
                    CurrencyCode = g.Key.Code,
                    CurrencySymbol = g.Key.Symbol,
                    TotalPremiums = g.Sum(p => p.PremiumAmount),
                    TotalCommissions = g.Sum(p => p.CommissionAmount),
                    PolicyCount = g.Count()
                })
                .OrderByDescending(c => c.TotalPremiums)
                .ToListAsync();
        }
    }
}