using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.MultiTenancy.Models;
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

        public async Task<Customer?> GetByIdentificationNoAsync(string IdentificationNo)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IdentificationNumber == IdentificationNo)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.CustomerCode == customerCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Customer>> GetCustomersWithActivePoliciesAsync()
        {
            return await _dbSet
                .Include(c => c.Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted))
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(c => c.Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted))
                    .ThenInclude(p => p.PolicyType)
                .Where(c => !c.IsDeleted &&
                       c.Policies.Any(p => p.Status == PolicyStatus.Active && !p.IsDeleted))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetActivePoliciesAsync(int customerId)
        {
            var customer = await _dbSet
                .Include(c => c.Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted))
                    .ThenInclude(p => p.InsuranceCompany)
                .Include(c => c.Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted))
                    .ThenInclude(p => p.PolicyType)
                .Where(c => c.Id == customerId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            return customer?.Policies.Where(p => p.Status == PolicyStatus.Active && !p.IsDeleted).ToList() ?? new List<Policy>();
        }

        public async Task<(List<Customer> customers, int totalCount)> GetPagedAsync(CustomerQueryParams queryParams)
        {
            var query = _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Where(c => !c.IsDeleted); //&& c.TenantId == queryParams.);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTerm = queryParams.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.IdentificationNumber.Contains(searchTerm) ||
                    c.CustomerCode.Contains(searchTerm));
            }

            // Apply status filter
            if (queryParams.Status.HasValue)
            {
                query = query.Where(c => c.Status == queryParams.Status.Value);
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

            // Include Policies for calculating aggregated data (ActivePoliciesCount, TotalPremium, etc.)
            // Filter out deleted policies to avoid loading unnecessary data
            query = query.Include(c => c.Policies.Where(p => !p.IsDeleted));

            // Apply pagination
            var customers = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return (customers, totalCount);
        }

        public async Task<bool> IdentificationNoExistsAsync(string IdentificationNo, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.IdentificationNumber == IdentificationNo);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Email == email);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CustomerCodeExistsAsync(string customerCode, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.CustomerCode == customerCode);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Customer>> GetCustomersCreatedBetweenAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var query = _dbSet.AsQueryable();

                query = query.Where(c => c.CreatedOn >= startDate &&
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
                        CreatedOn = c.CreatedOn
                    })
                    .OrderBy(c => c.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving customers created between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}", ex);
            }
        }

        public async Task<Customer?> GetLastCustomerAsync()
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted)
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

        public async Task<Customer?> GetByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            try
            {
                // Clean the phone number for comparison (remove formatting)
                var cleanedPhone = CleanPhoneNumber(phoneNumber);

                return await _dbSet
                    .Where(c => !c.IsDeleted &&
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

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted &&
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

        public async Task<int> GetCustomerCountAsync()
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                //_logger?.LogError(ex, "Error getting customer count for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<Customer>> GetRecentCustomersAsync(int count = 10)
        {
            try
            {
                // OPTIMIZED: AsNoTracking for read-only query, Include before OrderByDescending for better performance
                return await _dbSet
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Policies.Where(p => !p.IsDeleted))
                    .OrderByDescending(c => c.CreatedOn)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting recent customers for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<CustomerStatistics> GetCustomerStatisticsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var lastMonth = startOfMonth.AddMonths(-1);

                // OPTIMIZED: Use database aggregations instead of loading all customers
                var baseQuery = _dbSet.Where(c => !c.IsDeleted);

                var totalCustomers = await baseQuery.CountAsync();
                var activeCustomers = await baseQuery.CountAsync(c => c.Status == CustomerStatus.Active);
                var inactiveCustomers = await baseQuery.CountAsync(c => c.Status == CustomerStatus.Inactive);
                var newCustomersThisMonth = await baseQuery.CountAsync(c => c.CreatedOn >= startOfMonth);
                var newCustomersThisWeek = await baseQuery.CountAsync(c => c.CreatedOn >= startOfWeek);
                var newCustomersLastMonth = await baseQuery.CountAsync(c => c.CreatedOn >= lastMonth && c.CreatedOn < startOfMonth);


                // Calculate growth percentage
                var growthPercentage = newCustomersLastMonth > 0
                    ? ((decimal)(newCustomersThisMonth - newCustomersLastMonth) / newCustomersLastMonth) * 100
                    : newCustomersThisMonth > 0 ? 100 : 0;

                var customersWithActivePolicies = await _dbSet
                    .Where(c => !c.IsDeleted &&
                               c.Policies.Any(p => p.Status== PolicyStatus.Active && !p.IsDeleted))
                    .CountAsync();

                var customersWithoutPolicies = totalCustomers - customersWithActivePolicies;

                // Calculate average age
                var averageAge = await GetAverageCustomerAgeAsync();

                // Get status breakdown
                var customersByStatus = await GetCustomersByStatusAsync();

                // Get gender breakdown
                var customersByGender = await GetCustomersByGenderAsync();

                return new CustomerStatistics
                {
                    TotalCustomers = totalCustomers,
                    ActiveCustomers = activeCustomers,
                    InactiveCustomers = inactiveCustomers,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    NewCustomersThisWeek = newCustomersThisWeek,
                    CustomerGrowthPercentage = growthPercentage,
                    CustomersWithActivePolicies = customersWithActivePolicies,
                    CustomersWithoutPolicies = customersWithoutPolicies,
                    AverageCustomerAge = averageAge,
                    CustomersByStatus = customersByStatus,
                    CustomersByGender = customersByGender
                };
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting customer statistics for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<Customer>> GetTopCustomersByPolicyCountAsync(int count = 10)
        {
            try
            {
                // OPTIMIZED: AsNoTracking for read-only query, count in database, then load customers
                return await _dbSet
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted)
                    .Select(c => new {
                        Customer = c,
                        PolicyCount = c.Policies.Count(p => !p.IsDeleted)
                    })
                    .OrderByDescending(x => x.PolicyCount)
                    .Take(count)
                    .Select(x => x.Customer)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting top customers by policy count for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Dictionary<CustomerStatus, int>> GetCustomersByStatusAsync()
        {
            try
            {
                var statusCounts = await _dbSet
                    .Where(c => !c.IsDeleted)
                    .GroupBy(c => c.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return statusCounts.ToDictionary(x => x.Status, x => x.Count);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting customers by status for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetCustomersCreatedByMonthAsync(int months = 12)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);

                var monthlyData = await _dbSet
                    .Where(c => !c.IsDeleted && c.CreatedOn >= startDate)
                    .GroupBy(c => new { c.CreatedOn.Year, c.CreatedOn.Month })
                    .Select(g => new {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                return monthlyData.ToDictionary(
                    x => $"{x.Year}-{x.Month:D2}",
                    x => x.Count);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting customers by month for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<double> GetAverageCustomerAgeAsync()
        {
            try
            {
                var today = DateTime.Today;

                var ages = await _dbSet
                    .Where(c => !c.IsDeleted)
                    .Select(c => EF.Functions.DateDiffYear(c.DateOfBirth, today))
                    .ToListAsync();

                return (double)(ages.Any() ? ages.Average() : 0);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error calculating average customer age for tenant {TenantId}", tenantId);
                return 0;
            }
        }

        public async Task<Dictionary<Gender, int>> GetCustomersByGenderAsync()
        {
            try
            {
                var genderCounts = await _dbSet
                    .Where(c => !c.IsDeleted)
                    .GroupBy(c => c.Gender)
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToListAsync();

                return genderCounts.ToDictionary(x => x.Gender, x => x.Count);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting customers by gender for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<int> GetNewCustomersCountAsync(DateTime fromDate)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.CreatedOn >= fromDate)
                .CountAsync();
        }

        // ========================================
        // IQueryable Methods for AutoMapper ProjectTo
        // Return IQueryable to allow Application layer to project to DTOs
        // Keeps clean architecture - no Application -> Persistence dependency
        // ========================================

        public IQueryable<Customer> GetPagedQuery(CustomerQueryParams queryParams)
        {
            var query = _dbSet.Where(c => !c.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var search = queryParams.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(search) ||
                    c.LastName.ToLower().Contains(search) ||
                    c.IdentificationNumber.ToLower().Contains(search) ||
                    (c.Email != null && c.Email.ToLower().Contains(search)) ||
                    (c.Phone != null && c.Phone.Contains(search)));
            }

            // Apply status filter
            if (queryParams.Status.HasValue)
            {
                query = query.Where(c => c.Status == queryParams.Status.Value);
            }

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
            return query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize);
        }

        public IQueryable<Customer> GetRecentCustomersQuery(int count = 10)
        {
            return _dbSet
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedOn)
                .Take(count);
        }

        public IQueryable<Customer> GetCustomersWithActivePoliciesQuery()
        {
            return _dbSet
                .Where(c => !c.IsDeleted &&
                           c.Policies.Any(p => !p.IsDeleted && p.Status == PolicyStatus.Active))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName);
        }

        #region private methods
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


        #endregion
    }
}