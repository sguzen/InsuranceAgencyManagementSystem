using IAMS.Application.Models;
using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetCustomersWithPoliciesAsync();
        Task<Customer> GetCustomerWithMappingsAsync(int customerId);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<PagedResult<Customer>> GetCustomersPagedAsync(int pageNumber, int pageSize, string searchTerm = null);


        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<Customer?> GetByTcNoAsync(string tcNo);
    }
}
