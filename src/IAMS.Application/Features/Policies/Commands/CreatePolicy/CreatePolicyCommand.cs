using MediatR;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

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