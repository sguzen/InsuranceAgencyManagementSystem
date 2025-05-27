using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace IAMS.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(_context);
            Policies = new PolicyRepository(_context);
            InsuranceCompanies = new Repository<InsuranceCompany>(_context);
            PolicyTypes = new Repository<PolicyType>(_context);
            CommissionRates = new Repository<CommissionRate>(_context);
            PolicyPayments = new Repository<PolicyPayment>(_context);
            PolicyClaims = new Repository<PolicyClaim>(_context);
        }

        public ICustomerRepository Customers { get; }
        public IPolicyRepository Policies { get; }
        public IRepository<InsuranceCompany> InsuranceCompanies { get; }
        public IRepository<PolicyType> PolicyTypes { get; }
        public IRepository<CommissionRate> CommissionRates { get; }
        public IRepository<PolicyPayment> PolicyPayments { get; }
        public IRepository<PolicyClaim> PolicyClaims { get; }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}