using IAMS.Application.Interfaces.Repositories;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IInsuranceCompanyRepository InsuranceCompanies { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}