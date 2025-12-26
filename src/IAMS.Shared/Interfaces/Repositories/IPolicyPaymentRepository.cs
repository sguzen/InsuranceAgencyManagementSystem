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
        Task<Dictionary<int, decimal>> GetTotalPaymentsByPolicyIdsAsync(IEnumerable<int> policyIds);
        Task<DateTime?> GetLastPaymentDateAsync(int customerId);
        Task<IEnumerable<PolicyPayment>> GetPaymentsDueThisMonthAsync(int? limit = null);

        // Query methods that return IQueryable for ProjectTo optimization
        IQueryable<PolicyPayment> GetOverduePaymentsQuery();
        IQueryable<PolicyPayment> GetPaymentsByPolicyIdQuery(int policyId);
        IQueryable<PolicyPayment> GetPaymentsByDateRangeQuery(DateTime fromDate, DateTime toDate);
        IQueryable<PolicyPayment> GetPaymentsDueThisMonthQuery();
        IQueryable<PolicyPayment> GetPagedPaymentsQuery(int pageNumber, int pageSize, string? searchTerm = null);
    }
}