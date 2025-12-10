using IAMS.Domain.Entities;


namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IPolicyPaymentRepository : IRepository<PolicyPayment>
    {
        Task<PolicyPayment?> GetByIdWithRelationsAsync(int id);
        Task<IEnumerable<PolicyPayment>> GetPaymentsByPolicyIdAsync(int policyId);
        Task<IEnumerable<PolicyPayment>> GetOverduePaymentsAsync();
        Task<IEnumerable<PolicyPayment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId);
        Task<DateTime?> GetLastPaymentDateAsync(int customerId);
        Task<IEnumerable<PolicyPayment>> GetPaymentsDueThisMonthAsync();
        Task<Dictionary<int, decimal>> GetOutstandingBalanceByCustomerAsync();
        Task<decimal> GetTotalOutstandingBalanceByCustomerIdAsync(int customerId);
    }
}