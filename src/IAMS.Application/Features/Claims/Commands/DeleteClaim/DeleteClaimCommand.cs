using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Claims.Commands.DeleteClaim
{
    public class DeleteClaimCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public DeleteClaimCommand(int id)
        {
            Id = id;
        }
    }
}
