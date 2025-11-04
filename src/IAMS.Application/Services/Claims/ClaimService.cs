using IAMS.Application.DTOs.Claim;
using IAMS.Application.Features.Claims.Commands.CreateClaim;
using IAMS.Application.Features.Claims.Commands.DeleteClaim;
using IAMS.Application.Features.Claims.Commands.UpdateClaimStatus;
using IAMS.Application.Features.Claims.Queries.GetClaimById;
using IAMS.Application.Features.Claims.Queries.GetClaimsByPolicyId;
using IAMS.Application.Features.Claims.Queries.GetClaimsByStatus;
using IAMS.Application.Features.Claims.Queries.GetClaimsPaged;
using IAMS.Application.Features.Claims.Queries.GetTotalClaimAmountByPolicyId;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Services.Claims
{
    public class ClaimService : IClaimService
    {
        private readonly IMediator _mediator;

        public ClaimService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<List<PolicyClaimDto>> GetClaimsByPolicyIdAsync(int policyId)
        {
            return await _mediator.Send(new GetClaimsByPolicyIdQuery(policyId));
        }

        public async Task<PolicyClaimDto?> GetByIdAsync(int id)
        {
            return await _mediator.Send(new GetClaimByIdQuery(id));
        }

        public async Task<PolicyClaimDto> CreateAsync(CreatePolicyClaimDto claimDto)
        {
            var result = await _mediator.Send(new CreateClaimCommand(claimDto));
            if (!result.IsSuccess || result.Data == null)
            {
                throw new InvalidOperationException(result.Message);
            }
            return result.Data;
        }

        public async Task UpdateClaimStatusAsync(int id, ClaimStatus status)
        {
            var result = await _mediator.Send(new UpdateClaimStatusCommand(id, status));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var result = await _mediator.Send(new DeleteClaimCommand(id));
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        public async Task<List<PolicyClaimDto>> GetClaimsByStatusAsync(ClaimStatus status)
        {
            return await _mediator.Send(new GetClaimsByStatusQuery(status));
        }

        public async Task<PagedResult<PolicyClaimDto>> GetClaimsPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            return await _mediator.Send(new GetClaimsPagedQuery(pageNumber, pageSize, searchTerm));
        }

        public async Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId)
        {
            return await _mediator.Send(new GetTotalClaimAmountByPolicyIdQuery(policyId));
        }
    }
}
