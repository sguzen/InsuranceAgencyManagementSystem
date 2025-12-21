using IAMS.Shared.DTOs.Claim;
using MediatR;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsByPolicyId
{
    public class GetClaimsByPolicyIdQuery : IRequest<List<PolicyClaimDto>>
    {
        public int PolicyId { get; set; }

        public GetClaimsByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
