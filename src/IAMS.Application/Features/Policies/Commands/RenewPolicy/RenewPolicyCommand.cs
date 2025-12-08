using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Commands.RenewPolicy
{
    public class RenewPolicyCommand : IRequest<Result<PolicyDto>>
    {
        public RenewPolicyCommand(int originalPolicyId, DateTime newStartDate, DateTime newEndDate, decimal newPremiumAmount)
        {
            OriginalPolicyId = originalPolicyId;
            NewStartDate = newStartDate;
            NewEndDate = newEndDate;
            NewPremiumAmount = newPremiumAmount;
        }
        public int OriginalPolicyId { get; set; }
        public DateTime NewStartDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public decimal NewPremiumAmount { get; set; }
    }
}