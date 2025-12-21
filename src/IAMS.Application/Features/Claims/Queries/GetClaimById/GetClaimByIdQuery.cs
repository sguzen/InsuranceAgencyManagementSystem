using IAMS.Shared.DTOs.Claim;
using MediatR;

namespace IAMS.Application.Features.Claims.Queries.GetClaimById
{
    public class GetClaimByIdQuery : IRequest<PolicyClaimDto?>
    {
        public int Id { get; set; }

        public GetClaimByIdQuery(int id)
        {
            Id = id;
        }
    }
}
