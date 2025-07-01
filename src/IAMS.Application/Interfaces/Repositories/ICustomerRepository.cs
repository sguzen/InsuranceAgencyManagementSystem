using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
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
        Task<Customer?> GetByEmailAsync(string email, int tenantId);
        Task<Customer?> GetByCustomerCodeAsync(string customerCode, int tenantId);
        Task<List<Customer>> GetCustomersWithActivePoliciesAsync(int tenantId);
        Task<List<Policy>> GetActivePoliciesAsync(int customerId);
        Task<(List<Customer> customers, int totalCount)> GetPagedAsync(CustomerQueryParams queryParams);
        Task<bool> KktcNoExistsAsync(string kktcNo, int tenantId, int? excludeCustomerId = null);
        Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeCustomerId = null);
        Task<bool> CustomerCodeExistsAsync(string customerCode, int tenantId, int? excludeCustomerId = null);
    }
}
