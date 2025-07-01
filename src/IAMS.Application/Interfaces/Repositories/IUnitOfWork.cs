using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IPolicyTypeRepository PolicyTypes { get; }
        IInsuranceCompanyRepository InsuranceCompanies { get; }
        ICustomerInsuranceCompanyRepository CustomerInsuranceCompanies { get; }
        IPolicyPaymentRepository PolicyPayments { get; }
        IPolicyClaimRepository PolicyClaims { get; }
        ICommissionRateRepository CommissionRates { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
    }
}