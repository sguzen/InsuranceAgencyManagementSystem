using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsByPolicyId
{
    public class GetPaymentsByPolicyIdQuery : IRequest<Result<List<PolicyPaymentDto>>>
    {
        public int PolicyId { get; set; }

        public GetPaymentsByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
