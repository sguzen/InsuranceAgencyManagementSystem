using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Persistence.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithPoliciesAsync()
        {
            return await _dbSet
                .Include(c => c.Policies)
                .ThenInclude(p => p.InsuranceCompany)
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerWithMappingsAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.InsuranceCompanyMappings)
                .ThenInclude(m => m.InsuranceCompany)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            return await _dbSet
                .Where(c =>
                    c.FirstName.Contains(searchTerm) ||
                    c.LastName.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm) ||
                    c.CustomerCode.Contains(searchTerm))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<PagedResult<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            IQueryable<Customer> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(searchTerm) ||
                    c.LastName.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm) ||
                    c.CustomerCode.Contains(searchTerm));
            }

            query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
