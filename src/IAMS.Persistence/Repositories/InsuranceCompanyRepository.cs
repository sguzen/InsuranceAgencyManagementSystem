using IAMS.Application.Interfaces.Repositories;
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
            return await _context.Policies
                .Where(p => p.InsuranceCompanyId == companyId &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .SumAsync(p => p.PremiumAmount);
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
            return await _context.Policies
                .Where(p => p.InsuranceCompanyId == id &&
                           p.Status == PolicyStatus.Active && !p.IsDeleted)
                .SumAsync(p => p.CommissionAmount);
        }
    }
}