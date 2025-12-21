using IAMS.Shared.DTOs.Claim;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Claims.Commands.CreateClaim
{
    public class CreateClaimCommand : IRequest<Result<PolicyClaimDto>>
    {
        public CreatePolicyClaimDto ClaimDto { get; set; }

        public CreateClaimCommand(CreatePolicyClaimDto claimDto)
        {
            ClaimDto = claimDto;
        }
    }
}
