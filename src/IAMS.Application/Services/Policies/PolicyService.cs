using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Policies
{
    public class PolicyService : IPolicyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PolicyService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyDto>> GetAllAsync()
        {
            var policies = await _unitOfWork.Policies.GetAllAsync();
            return _mapper.Map<List<PolicyDto>>(policies);
        }

        public async Task<PolicyDto?> GetByIdAsync(int id)
        {
            var policy = await _unitOfWork.Policies.GetByIdAsync(id);
            return policy != null ? _mapper.Map<PolicyDto>(policy) : null;
        }

        public async Task<PolicyDto> CreateAsync(CreatePolicyDto policyDto)
        {
            try
            {
                // Validate customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(policyDto.CustomerId);
                if (customer == null)
                {
                    throw new InvalidOperationException("Customer not found.");
                }

                // Validate insurance company exists
                var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(policyDto.InsuranceCompanyId);
                if (insuranceCompany == null)
                {
                    throw new InvalidOperationException("Insurance company not found.");
                }

                // Validate policy type exists
                var policyType = await _unitOfWork.PolicyTypes.GetByIdAsync(policyDto.PolicyTypeId);
                if (policyType == null)
                {
                    throw new InvalidOperationException("Policy type not found.");
                }

                // Check if policy number is unique
                var existingPolicy = await _unitOfWork.Policies.FirstOrDefaultAsync(p => p.PolicyNumber == policyDto.PolicyNumber);
                if (existingPolicy != null)
                {
                    throw new InvalidOperationException("Policy number already exists.");
                }

                var policy = _mapper.Map<Policy>(policyDto);
                policy.CreatedOn = DateTime.UtcNow;

                // Calculate commission amount
                policy.CommissionAmount = policy.PremiumAmount * (policy.CommissionRate / 100);

                await _unitOfWork.Policies.AddAsync(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy created successfully with ID: {PolicyId}", policy.Id);
                return _mapper.Map<PolicyDto>(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdatePolicyDto policyDto)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(id);
                if (policy == null)
                {
                    throw new InvalidOperationException("Policy not found.");
                }

                _mapper.Map(policyDto, policy);
                policy.ModifiedOn = DateTime.UtcNow;

                // Recalculate commission amount
                policy.CommissionAmount = policy.PremiumAmount * (policy.CommissionRate / 100);

                _unitOfWork.Policies.Update(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy updated successfully with ID: {PolicyId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy with ID: {PolicyId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(id);
                if (policy == null)
                {
                    throw new InvalidOperationException("Policy not found.");
                }

                // Check if policy has payments or claims
                var hasPayments = await _unitOfWork.PolicyPayments.ExistsAsync(p => p.PolicyId == id);
                var hasClaims = await _unitOfWork.PolicyClaims.ExistsAsync(c => c.PolicyId == id);

                if (hasPayments || hasClaims)
                {
                    // Soft delete
                    policy.IsDeleted = true;
                    policy.ModifiedOn = DateTime.UtcNow;
                    _unitOfWork.Policies.Update(policy);
                }
                else
                {
                    // Hard delete
                    _unitOfWork.Policies.Remove(policy);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Policy deleted successfully with ID: {PolicyId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy with ID: {PolicyId}", id);
                throw;
            }
        }

        public async Task<List<PolicyDto>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            var policies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(customerId);
            return _mapper.Map<List<PolicyDto>>(policies);
        }

        public async Task<List<PolicyDto>> GetExpiringPoliciesAsync(DateTime date)
        {
            var policies = await _unitOfWork.Policies.GetExpiringPoliciesAsync(date);
            return _mapper.Map<List<PolicyDto>>(policies);
        }

        public async Task<PagedResult<PolicyDto>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            // This would need implementation in the repository layer
            // For now, return a basic implementation
            var allPolicies = await _unitOfWork.Policies.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                allPolicies = allPolicies.Where(p =>
                    p.PolicyNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Customer.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Customer.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = allPolicies.Count();
            var items = allPolicies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<PolicyDto>
            {
                Items = _mapper.Map<List<PolicyDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<decimal> CalculateCommissionAsync(int policyId)
        {
            var policy = await _unitOfWork.Policies.GetByIdAsync(policyId);
            if (policy == null)
            {
                throw new InvalidOperationException("Policy not found.");
            }

            return policy.PremiumAmount * (policy.CommissionRate / 100);
        }

        public async Task<bool> RenewPolicyAsync(int policyId, DateTime newEndDate)
        {
            try
            {
                var policy = await _unitOfWork.Policies.GetByIdAsync(policyId);
                if (policy == null)
                {
                    return false;
                }

                policy.EndDate = newEndDate;
                policy.Status = PolicyStatus.Active;
                policy.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Policies.Update(policy);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy renewed successfully with ID: {PolicyId}", policyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing policy with ID: {PolicyId}", policyId);
                return false;
            }
        }
    }
}