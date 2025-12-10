using IAMS.Shared.Interfaces.Repositories;
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

        public async Task<PolicyPayment?> GetByIdWithRelationsAsync(int id)
        {
            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Currency)
                .Where(pp => pp.Id == id && !pp.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PolicyPayment>> GetPaymentsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Currency)
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
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Currency)
                .Where(pp => !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Pending &&
                            pp.DueDate.HasValue &&
                            pp.DueDate.Value < today)
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
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Currency)
                .Where(pp => !pp.IsDeleted &&
                            pp.PaymentDate >= fromDate &&
                            pp.PaymentDate <= toDate)
                .OrderByDescending(pp => pp.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Where(pp => pp.PolicyId == policyId &&
                            !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Completed)
                .SumAsync(pp => pp.Amount);
        }

        public async Task<DateTime?> GetLastPaymentDateAsync(int customerId)
        {
            // Optimized: No Include needed when using Select
            return await _dbSet
                .Where(pp => !pp.IsDeleted &&
                            pp.Policy.CustomerId == customerId)
                .OrderByDescending(pp => pp.PaymentDate)
                .Select(pp => (DateTime?)pp.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PolicyPayment>> GetPaymentsDueThisMonthAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _dbSet
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(pp => pp.Policy)
                    .ThenInclude(p => p.Currency)
                .Where(pp => !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Pending &&
                            pp.DueDate.HasValue &&
                            pp.DueDate.Value >= startOfMonth &&
                            pp.DueDate.Value <= endOfMonth)
                .OrderBy(pp => pp.DueDate)
                .ToListAsync();
        }

        public async Task<Dictionary<int, decimal>> GetOutstandingBalanceByCustomerAsync()
        {
            // Get all policies with their payments
            var policiesWithPayments = await _context.Policies
                .Include(p => p.PolicyPayments)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    CustomerId = p.CustomerId,
                    PremiumAmount = p.PremiumAmount,
                    TotalPaid = p.PolicyPayments
                        .Where(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Completed)
                        .Sum(pp => pp.Amount)
                })
                .ToListAsync();

            // Group by customer and calculate outstanding balance
            return policiesWithPayments
                .GroupBy(p => p.CustomerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.PremiumAmount - p.TotalPaid)
                );
        }

        public async Task<decimal> GetTotalOutstandingBalanceByCustomerIdAsync(int customerId)
        {
            var policiesWithPayments = await _context.Policies
                .Include(p => p.PolicyPayments)
                .Where(p => !p.IsDeleted && p.CustomerId == customerId)
                .Select(p => new
                {
                    PremiumAmount = p.PremiumAmount,
                    TotalPaid = p.PolicyPayments
                        .Where(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Completed)
                        .Sum(pp => pp.Amount)
                })
                .ToListAsync();

            return policiesWithPayments.Sum(p => p.PremiumAmount - p.TotalPaid);
        }

        // ========================================
        // IQueryable Methods for AutoMapper ProjectTo
        // Return IQueryable to allow Application layer to project to DTOs
        // Keeps clean architecture - no Application -> Persistence dependency
        // ========================================

        public IQueryable<PolicyPayment> GetOverduePaymentsQuery()
        {
            var today = DateTime.Today;
            return _dbSet
                .Where(pp => !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Pending &&
                            pp.DueDate.HasValue &&
                            pp.DueDate.Value < today)
                .OrderBy(pp => pp.PaymentDate);
        }

        public IQueryable<PolicyPayment> GetPaymentsByPolicyIdQuery(int policyId)
        {
            return _dbSet
                .Where(pp => pp.PolicyId == policyId && !pp.IsDeleted)
                .OrderByDescending(pp => pp.PaymentDate);
        }

        public IQueryable<PolicyPayment> GetPaymentsByDateRangeQuery(DateTime fromDate, DateTime toDate)
        {
            return _dbSet
                .Where(pp => !pp.IsDeleted &&
                            pp.PaymentDate >= fromDate &&
                            pp.PaymentDate <= toDate)
                .OrderByDescending(pp => pp.PaymentDate);
        }

        public IQueryable<PolicyPayment> GetPaymentsDueThisMonthQuery()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return _dbSet
                .Where(pp => !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Pending &&
                            pp.DueDate.HasValue &&
                            pp.DueDate.Value >= startOfMonth &&
                            pp.DueDate.Value <= endOfMonth)
                .OrderBy(pp => pp.DueDate);
        }
    }
}
