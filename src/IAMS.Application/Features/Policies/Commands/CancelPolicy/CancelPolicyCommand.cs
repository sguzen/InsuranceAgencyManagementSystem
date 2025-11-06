using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.CancelPolicy
{
    public class CancelPolicyCommand : IRequest<Result<PolicyDto>>
    {
        public CancelPolicyCommand(int id, string? cancellationReason)
        {
            Id = id;    
            CancellationReason = cancellationReason;
        }
        public int Id { get; set; }
        public string? CancellationReason { get; set; }
    }
}