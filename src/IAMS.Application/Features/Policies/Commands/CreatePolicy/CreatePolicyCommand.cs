using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Commands.CreatePolicy
{
    public class CreatePolicyCommand : IRequest<Result<PolicyDto>>
    {
        public CreatePolicyDto PolicyDto { get; set; }

        public CreatePolicyCommand(CreatePolicyDto policyDto)
        {
            PolicyDto = policyDto;
        }
    }
}