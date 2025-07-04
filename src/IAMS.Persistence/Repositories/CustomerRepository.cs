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

        public async Task<List<Customer>> GetCustomersCreatedBetweenAsync(int tenantId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted &&
                               c.TenantId == tenantId &&
                               c.CreatedOn >= startDate &&
                               c.CreatedOn <= endDate)
                    .OrderBy(c => c.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if you have access to logger
                // _logger?.LogError(ex, "Error retrieving customers created between {StartDate} and {EndDate} for tenant {TenantId}", startDate, endDate, tenantId);
                throw; // Re-throw to let the caller handle it
            }
        }

        // Also add the GetLastCustomerAsync method that's referenced in the original code
        public async Task<Customer?> GetLastCustomerAsync(int tenantId)
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted && c.TenantId == tenantId)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if you have access to logger
                // _logger?.LogError(ex, "Error retrieving last customer for tenant {TenantId}", tenantId);
                throw;
            }
        }

        // And add the GetByPhoneAsync method that's also referenced
        public async Task<Customer?> GetByPhoneAsync(string phoneNumber, int tenantId)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            try
            {
                // Clean the phone number for comparison (remove formatting)
                var cleanedPhone = CleanPhoneNumber(phoneNumber);

                return await _dbSet
                    .Where(c => !c.IsDeleted &&
                               c.TenantId == tenantId &&
                               (c.Phone == phoneNumber ||
                                c.Phone == cleanedPhone ||
                                c.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "") == cleanedPhone))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if you have access to logger
                // _logger?.LogError(ex, "Error retrieving customer by phone {PhoneNumber} for tenant {TenantId}", phoneNumber, tenantId);
                throw;
            }
        }

        // Helper method to clean phone numbers for comparison
        private static string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            return phoneNumber
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "")
                .Replace(" ", "")
                .Replace("+", "")
                .Trim();
        }

        // Update the existing GetByEmailAsync to include tenantId parameter
        public async Task<Customer?> GetByEmailAsync(string email, int tenantId)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted &&
                               c.TenantId == tenantId &&
                               c.Email.ToLower() == email.ToLower())
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if you have access to logger
                // _logger?.LogError(ex, "Error retrieving customer by email {Email} for tenant {TenantId}", email, tenantId);
                throw;
            }
        }

        // Enhanced version with better performance for large datasets
        public async Task<List<Customer>> GetCustomersCreatedBetweenAsyncOptimized(int tenantId, DateTime startDate, DateTime endDate, bool includeDeleted = false)
        {
            try
            {
                var query = _dbSet.AsQueryable();

                // Apply filters
                if (!includeDeleted)
                {
                    query = query.Where(c => !c.IsDeleted);
                }

                query = query.Where(c => c.TenantId == tenantId &&
                                       c.CreatedOn >= startDate &&
                                       c.CreatedOn <= endDate);

                // For better performance with large datasets, only select required fields
                return await query
                    .Select(c => new Customer
                    {
                        Id = c.Id,
                        CustomerCode = c.CustomerCode,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Phone = c.Phone,
                        CreatedOn = c.CreatedOn,
                        TenantId = c.TenantId
                    })
                    .OrderBy(c => c.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception if you have access to logger
                throw new InvalidOperationException($"Error retrieving customers created between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd} for tenant {tenantId}", ex);
            }
        }

        // Bulk version for better performance when checking many customers
        public async Task<Dictionary<string, Customer>> GetCustomersByCodesAsync(List<string> customerCodes, int tenantId)
        {
            if (customerCodes == null || !customerCodes.Any())
                return new Dictionary<string, Customer>();

            try
            {
                var customers = await _dbSet
                    .Where(c => !c.IsDeleted &&
                               c.TenantId == tenantId &&
                               customerCodes.Contains(c.CustomerCode))
                    .ToListAsync();

                return customers.ToDictionary(c => c.CustomerCode, c => c);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customers by codes for tenant {tenantId}", ex);
            }
        }
    }
}