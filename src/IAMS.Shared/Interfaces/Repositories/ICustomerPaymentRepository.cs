using IAMS.Domain.Entities;
using System.Linq.Expressions;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface ICustomerPaymentRepository : IRepository<CustomerPayment>
    {
        /// <summary>
        /// Get customer payment by ID with all related data
        /// </summary>
        Task<CustomerPayment?> GetByIdWithRelationsAsync(int id);

        /// <summary>
        /// Get all payments for a specific customer
        /// </summary>
        Task<IEnumerable<CustomerPayment>> GetPaymentsByCustomerIdAsync(int customerId);

        /// <summary>
        /// Get payments within a date range for a customer
        /// </summary>
        Task<IEnumerable<CustomerPayment>> GetPaymentsByDateRangeAsync(int customerId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get unallocated or partially allocated payments for a customer
        /// </summary>
        Task<IEnumerable<CustomerPayment>> GetUnallocatedPaymentsAsync(int customerId);

        /// <summary>
        /// Calculate total debt for a customer in a specific currency
        /// </summary>
        Task<decimal> CalculateCustomerTotalDebtAsync(int customerId, int currencyId);

        /// <summary>
        /// Calculate total paid by customer in a specific currency
        /// </summary>
        Task<decimal> CalculateCustomerTotalPaidAsync(int customerId, int currencyId);

        /// <summary>
        /// Get customer balance (multi-currency)
        /// </summary>
        Task<Dictionary<int, (decimal TotalDebt, decimal TotalPaid, decimal Balance)>> GetCustomerBalanceAsync(int customerId);

        /// <summary>
        /// Get payments with allocations
        /// </summary>
        IQueryable<CustomerPayment> GetPaymentsWithAllocationsQuery(int customerId);
    }
}
