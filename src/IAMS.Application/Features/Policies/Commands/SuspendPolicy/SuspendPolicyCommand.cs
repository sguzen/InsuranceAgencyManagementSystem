using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

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