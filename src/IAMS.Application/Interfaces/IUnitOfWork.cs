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

        // Repository properties
        ICustomerRepository Customers { get; }
        IPolicyRepository Policies { get; }
        IRepository<InsuranceCompany> InsuranceCompanies { get; }
        IRepository<PolicyType> PolicyTypes { get; }
        IRepository<CommissionRate> CommissionRates { get; }
        IPolicyPaymentRepository PolicyPayments { get; }
        IPolicyClaimRepository PolicyClaims { get; }

        // Transaction methods
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Bulk operations
        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);
    }
}
