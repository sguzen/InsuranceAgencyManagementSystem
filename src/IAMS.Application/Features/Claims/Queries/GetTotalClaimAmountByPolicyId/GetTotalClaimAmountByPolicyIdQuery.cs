using MediatR;

namespace IAMS.Application.Features.Claims.Queries.GetTotalClaimAmountByPolicyId
{
    public class GetTotalClaimAmountByPolicyIdQuery : IRequest<decimal>
    {
        public int PolicyId { get; set; }

        public GetTotalClaimAmountByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
