using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IAMS.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Where(x => !x.IsDeleted).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(x => !x.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public virtual void Update(T entity)
        {
            entity.ModifiedOn = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.ModifiedOn = DateTime.UtcNow;
            }
            _dbSet.UpdateRange(entities);
        }

        public virtual void Remove(T entity)
        {
            // Soft delete
            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.ModifiedOn = DateTime.UtcNow;
            }
            _dbSet.UpdateRange(entities);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.Where(x => !x.IsDeleted).CountAsync();

            return await _dbSet.Where(x => !x.IsDeleted).CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(x => !x.IsDeleted).AnyAsync(predicate);
        }
    }

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<Customer?> GetByTcNoAsync(string tcNo)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.TcNo == tcNo);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(c => !c.IsDeleted &&
                       (c.FirstName.ToLower().Contains(term) ||
                        c.LastName.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term) ||
                        c.TcNo.Contains(term)))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }
    }

    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(DateTime date)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.EndDate <= date && p.Status == PolicyStatus.Active && !p.IsDeleted)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PolicyType)
                .Where(p => p.InsuranceCompanyId == insuranceCompanyId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
    }
}