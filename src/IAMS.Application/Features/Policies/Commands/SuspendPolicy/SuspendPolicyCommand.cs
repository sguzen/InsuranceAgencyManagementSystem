using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Commands.SuspendPolicy
{
    public class SuspendPolicyCommand : IRequest<Result<PolicyDto>>
    {
        public SuspendPolicyCommand(int id, string? suspensionReason)
        {
            Id = id;
            SuspensionReason = suspensionReason;
        }
        public int Id { get; set; }
        public string? SuspensionReason { get; set; }
    }
}