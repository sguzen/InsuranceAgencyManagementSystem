using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Commands.RenewPolicy
{
    public class RenewPolicyCommand : IRequest<Result<PolicyDto>>
    {
        public int OriginalPolicyId { get; set; }
        public DateTime NewStartDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public decimal NewPremiumAmount { get; set; }
    }
}