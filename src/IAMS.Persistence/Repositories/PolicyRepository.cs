using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Persistence.Repositories
{
    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(DateTime date)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.EndDate <= date && p.Status == PolicyStatus.Active && !p.IsDeleted)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PolicyType)
                .Where(p => p.InsuranceCompanyId == insuranceCompanyId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }
    }
}
