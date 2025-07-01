using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class PolicyTypeRepository : Repository<PolicyType>, IPolicyTypeRepository
    {
        public PolicyTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PolicyType>> GetActiveTypesAsync()
        {
            return await _dbSet
                .Where(pt => pt.IsActive && !pt.IsDeleted)
                .OrderBy(pt => pt.Name)
                .ToListAsync();
        }

        public async Task<PolicyType?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(pt => !pt.IsDeleted)
                .FirstOrDefaultAsync(pt => pt.Name == name);
        }

        public async Task<IEnumerable<PolicyType>> GetTypesByInsuranceCompanyAsync(int companyId)
        {
            return await _dbSet
                .Where(pt => pt.IsActive && !pt.IsDeleted &&
                            pt.CommissionRates.Any(cr => cr.InsuranceCompanyId == companyId &&
                                                        cr.IsActive && !cr.IsDeleted))
                .OrderBy(pt => pt.Name)
                .ToListAsync();
        }
    }
}