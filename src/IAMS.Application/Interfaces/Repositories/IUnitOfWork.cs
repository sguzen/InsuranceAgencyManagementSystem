using IAMS.Application.Interfaces.Repositories;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IPolicyTypeRepository PolicyTypes { get; }
        IInsuranceCompanyRepository InsuranceCompanies { get; }
        ICustomerInsuranceCompanyRepository CustomerInsuranceCompanies { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}