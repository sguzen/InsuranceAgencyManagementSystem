using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Commands.UpdatePolicy
{
    public class UpdatePolicyCommand : IRequest<Result<PolicyDto>>
    {
        public int Id { get; set; }
        public UpdatePolicyDto PolicyDto { get; set; }

        public UpdatePolicyCommand(int id, UpdatePolicyDto policyDto)
        {
            Id = id;
            PolicyDto = policyDto;
        }
    }
}