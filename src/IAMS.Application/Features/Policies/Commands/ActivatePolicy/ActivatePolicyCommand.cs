using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.ActivatePolicy
{
    public class ActivatePolicyCommand : IRequest<Result<PolicyDto>>
    {
        public int Id { get; set; }

        public ActivatePolicyCommand(int id)
        {
            Id = id;
        }
    }
}