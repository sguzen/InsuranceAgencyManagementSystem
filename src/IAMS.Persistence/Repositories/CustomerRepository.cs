using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IAMS.Persistence.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Policies)
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(c => c.Policies)
                    .ThenInclude(p => p.PolicyType)
                .Include(c => c.CustomerInsuranceCompanies)
                    .ThenInclude(m => m.InsuranceCompany)
                .Where(c => !c.IsDeleted && c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByKktcNoAsync(string kktcNo, int tenantId)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.KktcNo == kktcNo && c.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode, int tenantId)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.CustomerCode == customerCode && c.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Customer>> GetCustomersWithActivePoliciesAsync(int tenantId)
        {
            return await _dbSet
                .Include(c => c.Policies.Where(p => p.IsActive && !p.IsDeleted))
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(c => c.Policies.Where(p => p.IsActive && !p.IsDeleted))
                    .ThenInclude(p => p.PolicyType)
                .Where(c => !c.IsDeleted && c.TenantId == tenantId &&
                       c.Policies.Any(p => p.IsActive && !p.IsDeleted))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetActivePoliciesAsync(int customerId)
        {
            var customer = await _dbSet
                .Include(c => c.Policies.Where(p => p.IsActive && !p.IsDeleted))
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(c => c.Policies.Where(p => p.IsActive && !p.IsDeleted))
                    .ThenInclude(p => p.PolicyType)
                .Where(c => c.Id == customerId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            return customer?.Policies.Where(p => p.IsActive && !p.IsDeleted).ToList() ?? new List<Policy>();
        }

        public async Task<(List<Customer> customers, int totalCount)> GetPagedAsync(CustomerQueryParams queryParams)
        {
            var query = _dbSet.Where(c => !c.IsDeleted); //&& c.TenantId == queryParams.);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTerm = queryParams.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.KktcNo.Contains(searchTerm) ||
                    c.CustomerCode.Contains(searchTerm));
            }

            // Apply status filter
            if (queryParams.Status == Domain.Enums.CustomerStatus.Active)
            {
                query = query.Where(c => c.IsActive == true);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
            {
                var sortDirection = queryParams.SortDirection == "desc" ? "DESC" : "ASC";
                query = query.OrderBy($"{queryParams.SortBy} {sortDirection}");
            }
            else
            {
                query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
            }

            // Apply pagination
            var customers = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return (customers, totalCount);
        }

        public async Task<bool> KktcNoExistsAsync(string kktcNo, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.KktcNo == kktcNo && c.TenantId == tenantId);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Email == email && c.TenantId == tenantId);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CustomerCodeExistsAsync(string customerCode, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.CustomerCode == customerCode && c.TenantId == tenantId);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }
    }
}