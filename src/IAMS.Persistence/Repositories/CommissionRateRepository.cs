using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class CommissionRateRepository : Repository<CommissionRate>, ICommissionRateRepository
    {
        public CommissionRateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CommissionRate?> GetByPolicyTypeAndCompanyAsync(int policyTypeId, int companyId)
        {
            var currentDate = DateTime.Today;

            return await _dbSet
                .Include(cr => cr.PolicyType)
                .Include(cr => cr.InsuranceCompany)
                .Where(cr => cr.PolicyTypeId == policyTypeId &&
                            cr.InsuranceCompanyId == companyId &&
                            cr.IsActive && !cr.IsDeleted &&
                            cr.EffectiveDate <= currentDate &&
                            (cr.ExpiryDate == null || cr.ExpiryDate >= currentDate))
                .OrderByDescending(cr => cr.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CommissionRate>> GetByInsuranceCompanyAsync(int companyId)
        {
            return await _dbSet
                .Include(cr => cr.PolicyType)
                .Include(cr => cr.InsuranceCompany)
                .Where(cr => cr.InsuranceCompanyId == companyId && !cr.IsDeleted)
                .OrderBy(cr => cr.PolicyType.Name)
                .ThenByDescending(cr => cr.EffectiveDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommissionRate>> GetActiveFatesAsync()
        {
            var currentDate = DateTime.Today;

            return await _dbSet
                .Include(cr => cr.PolicyType)
                .Include(cr => cr.InsuranceCompany)
                .Where(cr => cr.IsActive && !cr.IsDeleted &&
                            cr.EffectiveDate <= currentDate &&
                            (cr.ExpiryDate == null || cr.ExpiryDate >= currentDate))
                .OrderBy(cr => cr.InsuranceCompany.Name)
                .ThenBy(cr => cr.PolicyType.Name)
                .ToListAsync();
        }
    }
}