using IAMS.Shared.DTOs.Claim;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsByStatus
{
    public class GetClaimsByStatusQuery : IRequest<List<PolicyClaimDto>>
    {
        public ClaimStatus Status { get; set; }

        public GetClaimsByStatusQuery(ClaimStatus status)
        {
            Status = status;
        }
    }
}
