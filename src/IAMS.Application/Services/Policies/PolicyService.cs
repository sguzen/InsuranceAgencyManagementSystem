using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Policies.Commands.ActivatePolicy;
using IAMS.Application.Features.Policies.Commands.CancelPolicy;
using IAMS.Application.Features.Policies.Commands.CreatePolicy;
using IAMS.Application.Features.Policies.Commands.DeletePolicy;
using IAMS.Application.Features.Policies.Commands.RenewPolicy;
using IAMS.Application.Features.Policies.Commands.SuspendPolicy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetExpiringPolicies;
using IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenueByCurrency;
using IAMS.Application.Features.Policies.Queries.GetPolicies;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer;
using IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus;
using IAMS.Application.Features.Policies.Queries.GetPolicy;
using IAMS.Application.Features.Policies.Queries.GetPolicyByNumber;
using IAMS.Application.Features.Policies.Queries.GetPolicyStatistics;
using IAMS.Application.Features.Policies.Queries.GetRecentPolicies;
using IAMS.Application.Features.Policies.Queries.GetRevenueByMonth;
using IAMS.Application.Features.Policies.Queries.GetTopPoliciesByPremium;
using IAMS.Application.Features.Policies.Queries.GetTotalPoliciesCount;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

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
            return await _mediator.Send(new GetPolicyByNumberQuery(policyNumber));
        }

        public async Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId)
        {
            return await _mediator.Send(new GetPoliciesByCustomerQuery(customerId));
        }

        // Dashboard statistics methods
        public async Task<Result<int>> GetTotalPoliciesCountAsync()
        {
            return await _mediator.Send(new GetTotalPoliciesCountQuery());
        }

        public async Task<Result<int>> GetExpiringPoliciesCountAsync(int daysAhead = 30)
        {
            return await _mediator.Send(new GetExpiringPoliciesCountQuery(daysAhead));
        }

        public async Task<Result<decimal>> GetMonthlyRevenueAsync()
        {
            return await _mediator.Send(new GetMonthlyRevenueQuery());
        }

        public async Task<Result<Dictionary<string, decimal>>> GetMonthlyRevenueByCurrencyAsync()
        {
            return await _mediator.Send(new GetMonthlyRevenueByCurrencyQuery());
        }

        public async Task<Result<PolicyStatisticsDto>> GetPolicyStatisticsAsync()
        {
            return await _mediator.Send(new GetPolicyStatisticsQuery());
        }

        public async Task<Result<List<PolicyDto>>> GetRecentPoliciesAsync(int count = 5)
        {
            return await _mediator.Send(new GetRecentPoliciesQuery(count));
        }

        public async Task<Result<Dictionary<PolicyStatus, int>>> GetPoliciesByStatusAsync()
        {
            return await _mediator.Send(new GetPoliciesByStatusQuery());
        }

        public async Task<Result<Dictionary<string, decimal>>> GetRevenueByMonthAsync(int months = 12)
        {
            return await _mediator.Send(new GetRevenueByMonthQuery(months));
        }

        public async Task<Result<List<PolicyDto>>> GetTopPoliciesByPremiumAsync(int count = 10)
        {
            return await _mediator.Send(new GetTopPoliciesByPremiumQuery(count));
        }

        public async Task<Result<List<PolicyDto>>> GetActivePoliciesAsync()
        {
            var queryParams = new PolicyQueryParams
            {
                Status = PolicyStatus.Active,
                PageSize = 1000,
                PageNumber = 1
            };
            var result = await GetPoliciesAsync(queryParams);
            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve active policies", result.Errors);
        }

        public async Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30)
        {
            return await _mediator.Send(new GetExpiringPoliciesQuery(daysAhead));
        }

        public async Task<Result<List<PolicyDto>>> GetExpiredPoliciesAsync()
        {
            var queryParams = new PolicyQueryParams
            {
                Status = PolicyStatus.Expired,
                PageSize = 1000,
                PageNumber = 1
            };
            var result = await GetPoliciesAsync(queryParams);
            return result.IsSuccess
                ? Result<List<PolicyDto>>.Success(result.Data?.Items ?? new List<PolicyDto>())
                : Result<List<PolicyDto>>.Failure(result.Message ?? "Failed to retrieve expired policies", result.Errors);
        }

        public async Task<Result<PolicyDto>> ActivatePolicyAsync(int id)
        {
            return await _mediator.Send(new ActivatePolicyCommand(id));
        }

        public async Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason = null)
        {
            return await _mediator.Send(new CancelPolicyCommand(id, reason));
        }

        public async Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason = null)
        {
            return await _mediator.Send(new SuspendPolicyCommand(id, reason));
        }

        public async Task<Result<PolicyDto>> RenewPolicyAsync(int id, CreatePolicyDto renewalDto)
        {
            return await _mediator.Send(new RenewPolicyCommand(id, renewalDto.StartDate.Value, renewalDto.EndDate.Value, renewalDto.PremiumAmount));
        }

        public async Task ProcessExpiringPoliciesAsync()
        {
            // This is typically a background job - get expiring policies and notify users
            // For now, just return - the actual implementation would be in a background service
            await Task.CompletedTask;
        }

        public async Task<Result<List<PolicyDto>>> GetEndorsementsByPolicyIdAsync(int policyId)
        {
            return await _mediator.Send(new GetEndorsementsByPolicyIdQuery(policyId));
        }
    }
}