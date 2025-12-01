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
        Task<Customer?> GetByIdentificationNoAsync(string IdentificationNo);
        Task<Customer?> GetByPhoneAsync(string phoneNumber);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        Task<List<Customer>> GetCustomersWithActivePoliciesAsync();
        Task<List<Policy>> GetActivePoliciesAsync(int customerId);
        Task<(List<Customer> customers, int totalCount)> GetPagedAsync(CustomerQueryParams queryParams);
        Task<bool> IdentificationNoExistsAsync(string IdentificationNo, int? excludeCustomerId = null);
        Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null);
        Task<bool> CustomerCodeExistsAsync(string customerCode, int? excludeCustomerId = null);
        Task<List<Customer>> GetCustomersCreatedBetweenAsync(DateTime startDate, DateTime endDate);

        // Dashboard and statistics methods
        Task<int> GetCustomerCountAsync();
        Task<List<Customer>> GetRecentCustomersAsync(int count = 10);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync();
        Task<List<Customer>> GetTopCustomersByPolicyCountAsync(int count = 10);
        Task<Dictionary<CustomerStatus, int>> GetCustomersByStatusAsync();
        Task<Dictionary<string, int>> GetCustomersCreatedByMonthAsync(int months = 12);
        Task<int> GetNewCustomersCountAsync(DateTime fromDate);
        Task<double> GetAverageCustomerAgeAsync();
        Task<Dictionary<Gender, int>> GetCustomersByGenderAsync();
        Task<Customer?> GetLastCustomerAsync();
    }
}
