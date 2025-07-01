using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class PolicyPaymentRepository : Repository<PolicyPayment>, IPolicyPaymentRepository
    {
        public PolicyPaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PolicyPayment>> GetPaymentsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pp => pp.PolicyId == policyId && !pp.IsDeleted)
                .OrderByDescending(pp => pp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyPayment>> GetOverduePaymentsAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pp => !pp.IsDeleted &&
                            pp.PaymentDate < today &&
                            pp.IsOverdue)
                .OrderBy(pp => pp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyPayment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pp => !pp.IsDeleted &&
                            pp.PaymentDate >= fromDate &&
                            pp.PaymentDate <= toDate)
                .OrderByDescending(pp => pp.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Where(pp => pp.PolicyId == policyId && !pp.IsDeleted && !pp.IsOverdue)
                .SumAsync(pp => pp.Amount);
        }
    }
}