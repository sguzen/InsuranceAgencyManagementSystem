using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Services.Policies
{
    public interface IPolicyService
    {
        Task<List<PolicyDto>> GetAllAsync();
        Task<PolicyDto?> GetByIdAsync(int id);
        Task<PolicyDto> CreateAsync(CreatePolicyDto policyDto);
        Task UpdateAsync(int id, UpdatePolicyDto policyDto);
        Task DeleteAsync(int id);
        Task<List<PolicyDto>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<List<PolicyDto>> GetExpiringPoliciesAsync(DateTime date);
        Task<PagedResult<PolicyDto>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<decimal> CalculateCommissionAsync(int policyId);
        Task<bool> RenewPolicyAsync(int policyId, DateTime newEndDate);
    }
}
