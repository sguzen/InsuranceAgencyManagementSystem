using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByIdWithDetailsAsync(int id);
        Task<Customer?> GetByKktcNoAsync(string kktcNo, int tenantId);
        Task<Customer?> GetByPhoneAsync(string phoneNumber, int tenantId);
        Task<Customer?> GetByEmailAsync(string email, int tenantId);
        Task<Customer?> GetByCustomerCodeAsync(string customerCode, int tenantId);
        Task<List<Customer>> GetCustomersWithActivePoliciesAsync(int tenantId);
        Task<List<Policy>> GetActivePoliciesAsync(int customerId, int tenandId);
        Task<(List<Customer> customers, int totalCount)> GetPagedAsync(CustomerQueryParams queryParams);
        Task<bool> KktcNoExistsAsync(string kktcNo, int tenantId, int? excludeCustomerId = null);
        Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeCustomerId = null);
        Task<bool> CustomerCodeExistsAsync(string customerCode, int tenantId, int? excludeCustomerId = null);
        Task<List<Customer>> GetCustomersCreatedBetweenAsync(int tenantId, DateTime startDate, DateTime endDate);

        // Dashboard and statistics methods
        Task<int> GetCustomerCountAsync(int tenantId);
        Task<List<Customer>> GetRecentCustomersAsync(int tenantId, int count = 10);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int tenantId);
        Task<List<Customer>> GetTopCustomersByPolicyCountAsync(int tenantId, int count = 10);
        Task<Dictionary<CustomerStatus, int>> GetCustomersByStatusAsync(int tenantId);
        Task<Dictionary<string, int>> GetCustomersCreatedByMonthAsync(int tenantId, int months = 12);
        Task<int> GetNewCustomersCountAsync(int tenantId, DateTime fromDate);
        Task<double> GetAverageCustomerAgeAsync(int tenantId);
        Task<Dictionary<Gender, int>> GetCustomersByGenderAsync(int tenantId);
        Task<Customer?> GetLastCustomerAsync(int tenantId);
    }
}
