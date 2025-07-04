using IAMS.Domain.Entities;


namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPolicyPaymentRepository : IRepository<PolicyPayment>
    {
        Task<IEnumerable<PolicyPayment>> GetPaymentsByPolicyIdAsync(int policyId);
        Task<IEnumerable<PolicyPayment>> GetOverduePaymentsAsync();
        Task<IEnumerable<PolicyPayment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId);
        Task<DateTime?> GetLastPaymentDateAsync(int customerId);
    }
}