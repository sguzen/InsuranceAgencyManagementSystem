using IAMS.Application.DTOs.Policy;
using IAMS.Application.Features.Policies.Commands.CreatePolicy;
using IAMS.Application.Features.Policies.Commands.DeletePolicy;
using IAMS.Application.Features.Policies.Commands.UpdatePolicy;
using IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount;
using IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue;
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

        public Task<Result<List<PolicyDto>>> GetActivePoliciesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<PolicyDto>>> GetExpiredPoliciesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<PolicyDto>> ActivatePolicyAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason = null)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason = null)
        {
            throw new NotImplementedException();
        }

        public Task<Result<PolicyDto>> RenewPolicyAsync(int id, CreatePolicyDto renewalDto)
        {
            throw new NotImplementedException();
        }

        public Task ProcessExpiringPoliciesAsync()
        {
            throw new NotImplementedException();
        }
    }
}