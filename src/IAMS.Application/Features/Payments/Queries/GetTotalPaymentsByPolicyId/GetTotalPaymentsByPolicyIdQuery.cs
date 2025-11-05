using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId
{
    public class GetTotalPaymentsByPolicyIdQuery : IRequest<decimal>
    {
        public int PolicyId { get; set; }

        public GetTotalPaymentsByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
