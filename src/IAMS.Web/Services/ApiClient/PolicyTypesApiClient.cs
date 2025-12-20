using IAMS.Application.DTOs.PolicyType;
using IAMS.Application.Models;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IPolicyTypesApiClient
    {
        Task<Result<List<PolicyTypeDto>>> GetAllAsync();
        Task<Result<List<PolicyTypeDto>>> GetActiveAsync();
        Task<Result<List<PolicyTypeDto>>> GetActiveTypesAsync();
        Task<Result<PolicyTypeDto>> GetByIdAsync(int id);
        Task<Result<PolicyTypeDto>> CreateAsync(CreatePolicyTypeDto policyTypeDto);
        Task<Result> UpdateAsync(int id, UpdatePolicyTypeDto policyTypeDto);
        Task<Result> DeleteAsync(int id);
    }

    public class PolicyTypesApiClient : BaseApiClient, IPolicyTypesApiClient
    {
        public PolicyTypesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<PolicyTypeDto>>> GetAllAsync()
        {
            return await GetAsync<List<PolicyTypeDto>>("api/policytypes");
        }

        public async Task<Result<List<PolicyTypeDto>>> GetActiveAsync()
        {
            return await GetAsync<List<PolicyTypeDto>>("api/policytypes/active");
        }

        public async Task<Result<List<PolicyTypeDto>>> GetActiveTypesAsync()
        {
            return await GetAsync<List<PolicyTypeDto>>("api/policytypes/active");
        }

        public async Task<Result<PolicyTypeDto>> GetByIdAsync(int id)
        {
            return await GetAsync<PolicyTypeDto>($"api/policytypes/{id}");
        }

        public async Task<Result<PolicyTypeDto>> CreateAsync(CreatePolicyTypeDto policyTypeDto)
        {
            return await PostAsync<PolicyTypeDto>("api/policytypes", policyTypeDto);
        }

        public async Task<Result> UpdateAsync(int id, UpdatePolicyTypeDto policyTypeDto)
        {
            return await PutAsync($"api/policytypes/{id}", policyTypeDto);
        }

        public async Task<Result> DeleteAsync(int id)
        {
            return await base.DeleteAsync($"api/policytypes/{id}");
        }
    }
}
