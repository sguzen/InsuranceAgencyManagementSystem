using MediatR;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Commands.DeletePolicy
{
    public class DeletePolicyCommand : IRequest<Result> 
    {
        public int Id { get; set; }

        public DeletePolicyCommand(int id)
        {
            Id = id;
        }
    }
}