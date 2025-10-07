using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
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

        public async Task<Customer?> GetByKktcNoAsync(string KktcNo, int tenantId)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.KktcNo == KktcNo)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode, int tenantId)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.CustomerCode == customerCode)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Customer>> GetCustomersWithActivePoliciesAsync(int tenantId)
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

        public async Task<List<Policy>> GetActivePoliciesAsync(int customerId, int tenantId)
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
                //query = query.Where(c => c.IsActive == true);
                query = query.Where(c => c.Status == Domain.Enums.CustomerStatus.Active && !c.IsDeleted);
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

        public async Task<bool> KktcNoExistsAsync(string KktcNo, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.KktcNo == KktcNo);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.Email == email);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> CustomerCodeExistsAsync(string customerCode, int tenantId, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => !c.IsDeleted && c.CustomerCode == customerCode);

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
                throw new InvalidOperationException($"Error retrieving customers created between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd} for tenant {tenantId}", ex);
            }
        }

        public async Task<Customer?> GetLastCustomerAsync(int tenantId)
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

        public async Task<Customer?> GetByEmailAsync(string email, int tenantId)
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

        public async Task<int> GetCustomerCountAsync(int tenantId)
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

        public async Task<List<Customer>> GetRecentCustomersAsync(int tenantId, int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedOn)
                    .Take(count)
                    .Include(c => c.Policies.Where(p => !p.IsDeleted))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting recent customers for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var lastMonth = startOfMonth.AddMonths(-1);

                var allCustomers = await _dbSet.Where(c => !c.IsDeleted).ToListAsync();

                var totalCustomers = allCustomers.Count;
                var activeCustomers = allCustomers.Count(c => c.Status == CustomerStatus.Active);
                var inactiveCustomers = allCustomers.Count(c => c.Status == CustomerStatus.Inactive);
                var newCustomersThisMonth = allCustomers.Count(c => c.CreatedOn >= startOfMonth);
                var newCustomersThisWeek = allCustomers.Count(c => c.CreatedOn >= startOfWeek);
                var newCustomersLastMonth = allCustomers.Count(c => c.CreatedOn >= lastMonth && c.CreatedOn < startOfMonth);


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
                var averageAge = await GetAverageCustomerAgeAsync(tenantId);

                // Get status breakdown
                var customersByStatus = await GetCustomersByStatusAsync(tenantId);

                // Get gender breakdown
                var customersByGender = await GetCustomersByGenderAsync(tenantId);

                return new CustomerStatisticsDto
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

        public async Task<List<Customer>> GetTopCustomersByPolicyCountAsync(int tenantId, int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Policies.Where(p => !p.IsDeleted))
                    .OrderByDescending(c => c.Policies.Count(p => !p.IsDeleted))
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting top customers by policy count for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Dictionary<CustomerStatus, int>> GetCustomersByStatusAsync(int tenantId)
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

        public async Task<Dictionary<string, int>> GetCustomersCreatedByMonthAsync(int tenantId, int months = 12)
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

        public async Task<double> GetAverageCustomerAgeAsync(int tenantId)
        {
            try
            {
                var today = DateTime.Today;

                var ages = await _dbSet
                    .Where(c => !c.IsDeleted)
                    .Select(c => EF.Functions.DateDiffYear(c.DateOfBirth, today))
                    .ToListAsync();

                return ages.Any() ? ages.Average() : 0;
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error calculating average customer age for tenant {TenantId}", tenantId);
                return 0;
            }
        }

        public async Task<Dictionary<Gender, int>> GetCustomersByGenderAsync(int tenantId)
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

        public Task<int> GetNewCustomersCountAsync(int tenantId, DateTime fromDate)
        {
            throw new NotImplementedException();
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