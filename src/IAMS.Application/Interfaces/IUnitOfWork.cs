using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        // Bulk operations
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);

        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IRepository<InsuranceCompany> InsuranceCompanies { get; }
        IRepository<PolicyType> PolicyTypes { get; }
        IRepository<CommissionRate> CommissionRates { get; }
        IRepository<PolicyPayment> PolicyPayments { get; }
        IRepository<PolicyClaim> PolicyClaims { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
