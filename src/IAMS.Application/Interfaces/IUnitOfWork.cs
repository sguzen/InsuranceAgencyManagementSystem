using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IInsuranceCompanyRepository InsuranceCompanies { get; }
        IPolicyTypeRepository PolicyTypes { get; }
        ICommissionRateRepository CommissionRates { get; }
        IPolicyPaymentRepository PolicyPayments { get; }
        IPolicyClaimRepository PolicyClaims { get; }
        ICustomerInsuranceCompanyRepository CustomerInsuranceCompanies { get; }

        // Transaction methods
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Bulk operations
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);

        // Tenant context (for multi-tenancy)
        int? TenantId { get; }
        void SetTenantId(int tenantId);
    }
}