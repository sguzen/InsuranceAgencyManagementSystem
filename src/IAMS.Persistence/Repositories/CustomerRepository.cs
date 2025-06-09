using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
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

        public async Task<IEnumerable<Customer>> GetCustomersWithPoliciesAsync()
        {
            return await _dbSet
                .Include(c => c.Policies)
                .ThenInclude(p => p.InsuranceCompany)
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerWithMappingsAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.CustomerInsuranceCompanies)
                .ThenInclude(m => m.InsuranceCompany)
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public Task<PagedResult<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            throw new NotImplementedException();
        }
    }

  
}