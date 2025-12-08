using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

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