using IAMS.Application.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsByPolicyId
{
    public class GetPaymentsByPolicyIdQuery : IRequest<List<PolicyPaymentDto>>
    {
        public int PolicyId { get; set; }

        public GetPaymentsByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
