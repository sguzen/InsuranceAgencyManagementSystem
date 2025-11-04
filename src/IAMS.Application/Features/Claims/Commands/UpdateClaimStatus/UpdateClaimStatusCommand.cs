using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Claims.Commands.UpdateClaimStatus
{
    public class UpdateClaimStatusCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public ClaimStatus Status { get; set; }

        public UpdateClaimStatusCommand(int id, ClaimStatus status)
        {
            Id = id;
            Status = status;
        }
    }
}
