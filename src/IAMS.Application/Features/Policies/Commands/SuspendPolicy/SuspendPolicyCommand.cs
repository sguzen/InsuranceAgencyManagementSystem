using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.SuspendPolicy
{
    public class SuspendPolicyCommand : IRequest<Result<PolicyDto>>
    {
        public int Id { get; set; }
        public string? SuspensionReason { get; set; }
    }
}