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

        public async Task<Dictionary<int, decimal>> GetTotalPaymentsByPolicyIdsAsync(IEnumerable<int> policyIds)
        {
            var policyIdsList = policyIds.ToList();
            if (!policyIdsList.Any())
            {
                return new Dictionary<int, decimal>();
            }

            // Use a single query to get total payments for all policies at once
            var result = await _dbSet
                .Where(pp => policyIdsList.Contains(pp.PolicyId) &&
                            !pp.IsDeleted &&
                            pp.Status == Domain.Enums.PaymentStatus.Completed)
                .GroupBy(pp => pp.PolicyId)
                .Select(g => new { PolicyId = g.Key, Total = g.Sum(pp => pp.Amount) })
                .ToDictionaryAsync(x => x.PolicyId, x => x.Total);

            return result;
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

        public async Task<IEnumerable<PolicyPayment>> GetPaymentsDueThisMonthAsync(int? limit = null)
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var query = _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
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
                .OrderBy(pp => pp.DueDate);

            // Apply limit if specified (for dashboard use)
            if (limit.HasValue && limit.Value > 0)
            {
                query = (IOrderedQueryable<PolicyPayment>)query.Take(limit.Value);
            }

            return await query.ToListAsync();
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

        public IQueryable<PolicyPayment> GetPagedPaymentsQuery(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.Where(pp => !pp.IsDeleted);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(pp =>
                    (pp.Reference != null && pp.Reference.ToLower().Contains(search)) ||
                    (pp.Notes != null && pp.Notes.ToLower().Contains(search)) ||
                    pp.Policy.PolicyNumber.ToLower().Contains(search) ||
                    (pp.Policy.Customer.FirstName + " " + pp.Policy.Customer.LastName).ToLower().Contains(search));
            }

            // Apply sorting and pagination
            return query
                .OrderByDescending(pp => pp.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
