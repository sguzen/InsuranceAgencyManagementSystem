using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class CustomerPaymentRepository : Repository<CustomerPayment>, ICustomerPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerPaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CustomerPayment?> GetByIdWithRelationsAsync(int id)
        {
            return await _dbSet
                .Include(cp => cp.Customer)
                .Include(cp => cp.Currency)
                .Include(cp => cp.PaymentAllocations)
                    .ThenInclude(pa => pa.Policy)
                .FirstOrDefaultAsync(cp => cp.Id == id && !cp.IsDeleted);
        }

        public async Task<IEnumerable<CustomerPayment>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(cp => cp.Currency)
                .Include(cp => cp.PaymentAllocations)
                .Where(cp => cp.CustomerId == customerId && !cp.IsDeleted)
                .OrderByDescending(cp => cp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerPayment>> GetPaymentsByDateRangeAsync(int customerId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(cp => cp.Currency)
                .Include(cp => cp.PaymentAllocations)
                .Where(cp => cp.CustomerId == customerId &&
                            !cp.IsDeleted &&
                            cp.PaymentDate >= fromDate &&
                            cp.PaymentDate <= toDate)
                .OrderByDescending(cp => cp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomerPayment>> GetUnallocatedPaymentsAsync(int customerId)
        {
            return await _dbSet
                .Include(cp => cp.Currency)
                .Where(cp => cp.CustomerId == customerId &&
                            !cp.IsDeleted &&
                            cp.Status == PaymentStatus.Completed &&
                            (cp.AllocationStatus == AllocationStatus.Unallocated ||
                             cp.AllocationStatus == AllocationStatus.PartiallyAllocated))
                .OrderBy(cp => cp.PaymentDate) // FIFO - oldest first
                .ToListAsync();
        }

        public async Task<decimal> CalculateCustomerTotalDebtAsync(int customerId, int currencyId)
        {
            // Sum of all policy premiums for this customer in the specified currency
            return await _context.Policies
                .Where(p => p.CustomerId == customerId &&
                           p.CurrencyId == currencyId &&
                           !p.IsDeleted)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<decimal> CalculateCustomerTotalPaidAsync(int customerId, int currencyId)
        {
            // Sum of all completed customer payments in the specified currency
            var customerPaymentsTotal = await _dbSet
                .Where(cp => cp.CustomerId == customerId &&
                            cp.CurrencyId == currencyId &&
                            cp.Status == PaymentStatus.Completed &&
                            !cp.IsDeleted)
                .SumAsync(cp => (decimal?)cp.Amount) ?? 0;

            // Sum of all completed policy payments for this customer's policies in the specified currency
            var policyPaymentsTotal = await _context.PolicyPayments
                .Where(pp => pp.Policy.CustomerId == customerId &&
                            pp.CurrencyId == currencyId &&
                            pp.Status == PaymentStatus.Completed &&
                            !pp.IsDeleted)
                .SumAsync(pp => (decimal?)pp.Amount) ?? 0;

            return customerPaymentsTotal + policyPaymentsTotal;
        }

        public async Task<Dictionary<int, (decimal TotalDebt, decimal TotalPaid, decimal Balance)>> GetCustomerBalanceAsync(int customerId)
        {
            // Get all currencies used by this customer
            var policyCurrencies = await _context.Policies
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .Select(p => p.CurrencyId)
                .Distinct()
                .ToListAsync();

            var customerPaymentCurrencies = await _dbSet
                .Where(cp => cp.CustomerId == customerId && !cp.IsDeleted && cp.Status == PaymentStatus.Completed)
                .Select(cp => cp.CurrencyId)
                .Distinct()
                .ToListAsync();

            var policyPaymentCurrencies = await _context.PolicyPayments
                .Where(pp => pp.Policy.CustomerId == customerId && !pp.IsDeleted && pp.Status == PaymentStatus.Completed)
                .Select(pp => pp.CurrencyId)
                .Distinct()
                .ToListAsync();

            var allCurrencies = policyCurrencies
                .Union(customerPaymentCurrencies)
                .Union(policyPaymentCurrencies)
                .Distinct()
                .ToList();

            var balances = new Dictionary<int, (decimal TotalDebt, decimal TotalPaid, decimal Balance)>();

            foreach (var currencyId in allCurrencies)
            {
                var totalDebt = await CalculateCustomerTotalDebtAsync(customerId, currencyId);
                var totalPaid = await CalculateCustomerTotalPaidAsync(customerId, currencyId);
                var balance = totalDebt - totalPaid;

                balances[currencyId] = (totalDebt, totalPaid, balance);
            }

            return balances;
        }

        public IQueryable<CustomerPayment> GetPaymentsWithAllocationsQuery(int customerId)
        {
            return _dbSet
                .Include(cp => cp.Currency)
                .Include(cp => cp.PaymentAllocations)
                    .ThenInclude(pa => pa.Policy)
                        .ThenInclude(p => p.PolicyType)
                .Where(cp => cp.CustomerId == customerId && !cp.IsDeleted)
                .OrderByDescending(cp => cp.PaymentDate);
        }
    }
}
