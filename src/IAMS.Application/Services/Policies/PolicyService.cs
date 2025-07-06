using MediatR;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Application.Features.Policies.Commands.CreatePolicy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Commands.DeletePolicy;
using IAMS.Application.Features.Policies.Queries.GetPolicy;
using IAMS.Application.Features.Policies.Queries.GetPolicies;
using IAMS.Domain.Enums;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
using IAMS.Application.Features.Policies.Queries.GetTotalPoliciesCount;
using IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount;

namespace IAMS.Application.Services.Policies
{
    public class PolicyService : IPolicyService
    {
        private readonly IMediator _mediator;

        public PolicyService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result<PolicyDto>> GetPolicyByIdAsync(int id)
        {
            return await _mediator.Send(new GetPolicyQuery(id));
        }

        public async Task<Result<PagedResult<PolicyDto>>> GetPoliciesAsync(PolicyQueryParams queryParams)
        {
            return await _mediator.Send(new GetPoliciesQuery(queryParams));
        }

        public async Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto createPolicyDto)
        {
            return await _mediator.Send(new CreatePolicyCommand(createPolicyDto));
        }

        public async Task<Result<PolicyDto>> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto)
        {
            return await _mediator.Send(new UpdatePolicyCommand(id, updatePolicyDto));
        }

        public async Task<Result> DeletePolicyAsync(int id)
        {
            return await _mediator.Send(new DeletePolicyCommand(id));
        }

        public async Task<Result<PolicyDto>> GetPolicyByNumberAsync(string policyNumber)
        {
            var queryParams = new PolicyQueryParams { SearchTerm = policyNumber, PageSize = 1 };
            var result = await GetPoliciesAsync(queryParams);

            if (result.IsSuccess && result.Data?.Items.Any() == true)
            {
                var policy = result.Data.Items.FirstOrDefault(p => p.PolicyNumber == policyNumber);
                return policy != null
                    ? Result<PolicyDto>.Success(policy)
                    : Result<PolicyDto>.NotFound("Policy not found");
            }

            return Result<PolicyDto>.NotFound("Policy not found");
        }

        public async Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId)
        {
            var queryParams = new PolicyQueryParams { CustomerId = customerId, PageSize = 1000 };
            var result = await GetPoliciesAsync(queryParams);

            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve policies", result.Errors);
        }

        public async Task<Result<List<PolicyDto>>> GetActivePoliciesAsync()
        {
            var queryParams = new PolicyQueryParams
            {
                Status = Domain.Enums.PolicyStatus.Active,
                PageSize = 1000
            };
            var result = await GetPoliciesAsync(queryParams);

            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve active policies", result.Errors);
        }

        public async Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30)
        {
            var queryParams = new PolicyQueryParams
            {
                Status = Domain.Enums.PolicyStatus.Active,
                EndDateFrom = DateTime.Today,
                EndDateTo = DateTime.Today.AddDays(daysAhead),
                PageSize = 1000
            };
            var result = await GetPoliciesAsync(queryParams);

            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve expiring policies", result.Errors);
        }

        public async Task<Result<List<PolicyDto>>> GetExpiredPoliciesAsync()
        {
            var queryParams = new PolicyQueryParams
            {
                IsExpired = true,
                PageSize = 1000
            };
            var result = await GetPoliciesAsync(queryParams);

            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve expired policies", result.Errors);
        }

        public async Task<Result<PolicyDto>> ActivatePolicyAsync(int id)
        {
            // This would need a specific ActivatePolicyCommand
            var policy = await GetPolicyByIdAsync(id);
            if (!policy.IsSuccess || policy.Data == null)
                return Result<PolicyDto>.NotFound("Policy not found");

            // For now, using update with status change
            var updateDto = new UpdatePolicyDto
            {
                StartDate = policy.Data.StartDate,
                EndDate = policy.Data.EndDate,
                PremiumAmount = policy.Data.PremiumAmount,
                CommissionRate = policy.Data.CommissionRate,
                Status = Domain.Enums.PolicyStatus.Active,
                Notes = policy.Data.Notes
            };

            return await UpdatePolicyAsync(id, updateDto);
        }

        public async Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason = null)
        {
            var policy = await GetPolicyByIdAsync(id);
            if (!policy.IsSuccess || policy.Data == null)
                return Result<PolicyDto>.NotFound("Policy not found");

            var updateDto = new UpdatePolicyDto
            {
                StartDate = policy.Data.StartDate,
                EndDate = policy.Data.EndDate,
                PremiumAmount = policy.Data.PremiumAmount,
                CommissionRate = policy.Data.CommissionRate,
                Status = Domain.Enums.PolicyStatus.Cancelled,
                Notes = string.IsNullOrEmpty(reason) ? policy.Data.Notes : $"{policy.Data.Notes}\nCancellation reason: {reason}"
            };

            return await UpdatePolicyAsync(id, updateDto);
        }

        public async Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason = null)
        {
            var policy = await GetPolicyByIdAsync(id);
            if (!policy.IsSuccess || policy.Data == null)
                return Result<PolicyDto>.NotFound("Policy not found");

            var updateDto = new UpdatePolicyDto
            {
                StartDate = policy.Data.StartDate,
                EndDate = policy.Data.EndDate,
                PremiumAmount = policy.Data.PremiumAmount,
                CommissionRate = policy.Data.CommissionRate,
                Status = Domain.Enums.PolicyStatus.Suspended,
                Notes = string.IsNullOrEmpty(reason) ? policy.Data.Notes : $"{policy.Data.Notes}\nSuspension reason: {reason}"
            };

            return await UpdatePolicyAsync(id, updateDto);
        }

        public async Task<Result<PolicyDto>> RenewPolicyAsync(int id, CreatePolicyDto renewalDto)
        {
            var originalPolicy = await GetPolicyByIdAsync(id);
            if (!originalPolicy.IsSuccess || originalPolicy.Data == null)
                return Result<PolicyDto>.NotFound("Original policy not found");

            if (!originalPolicy.Data.CanRenew)
                return Result<PolicyDto>.InternalError("Policy cannot be renewed in its current state");

            // Set renewal-specific properties
            renewalDto.PolicyNumber = $"{originalPolicy.Data.PolicyNumber}-R{DateTime.Now:yyyyMMdd}";
            renewalDto.CustomerId = originalPolicy.Data.CustomerId;
            renewalDto.InsuranceCompanyId = originalPolicy.Data.InsuranceCompanyId;
            renewalDto.PolicyTypeId = originalPolicy.Data.PolicyTypeId;
            renewalDto.Notes = $"Renewal of policy {originalPolicy.Data.PolicyNumber}";

            return await CreatePolicyAsync(renewalDto);
        }

        public Task ProcessExpiringPoliciesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<int>> GetTotalPoliciesCountAsync()
        {
            try
            {
                var query = new GetTotalPoliciesCountQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<int>.InternalError($"Poliçe sayısı alınırken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result<int>> GetExpiringPoliciesCountAsync(int daysAhead = 30)
        {
            try
            {
                var query = new GetExpiringPoliciesCountQuery(daysAhead);
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<int>.InternalError($"Süresi dolacak poliçe sayısı alınırken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result<decimal>> GetMonthlyRevenueAsync()
        {
            try
            {
                var query = new GetMonthlyRevenueQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<decimal>.InternalError($"Aylık gelir alınırken hata oluştu: {ex.Message}");
            }
        }

        public async Task<Result<PolicyStatisticsDto>> GetPolicyStatisticsAsync()
        {
            try
            {
                var query = new GetPolicyStatisticsQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<PolicyStatisticsDto>.InternalError($"Poliçe istatistikleri alınırken hata oluştu: {ex.Message}");
            }
        }

        public Task<Result<List<PolicyDto>>> GetRecentPoliciesAsync(int count = 5)
        {
            try
            {
                var query = new GetRecentPoliciesQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<PolicyStatisticsDto>.InternalError($"Poliçe istatistikleri alınırken hata oluştu: {ex.Message}");
            }
        }

        public Task<Result<Dictionary<PolicyStatus, int>>> GetPoliciesByStatusAsync()
        {
            try
            {
                var query = new GetPoliciesByStatusQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<PolicyStatisticsDto>.InternalError($"Poliçe istatistikleri alınırken hata oluştu: {ex.Message}");
            }
        }

        public Task<Result<Dictionary<string, decimal>>> GetRevenueByMonthAsync(int months = 12)
        {
            try
            {
                var query = new GetRevenueByMonthQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<PolicyStatisticsDto>.InternalError($"Poliçe istatistikleri alınırken hata oluştu: {ex.Message}");
            }
        }

        public Task<Result<List<PolicyDto>>> GetTopPoliciesByPremiumAsync(int count = 10)
        {
            try
            {
                var query = new GetTopPoliciesByPremiumQuery();
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                return Result<PolicyStatisticsDto>.InternalError($"Poliçe istatistikleri alınırken hata oluştu: {ex.Message}");
            }
        }
    }
}