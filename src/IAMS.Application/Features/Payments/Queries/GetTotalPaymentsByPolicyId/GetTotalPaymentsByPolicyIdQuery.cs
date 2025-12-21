using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId
{
    public class GetTotalPaymentsByPolicyIdQuery : IRequest<Result<decimal>>
    {
        public int PolicyId { get; set; }

        public GetTotalPaymentsByPolicyIdQuery(int policyId)
        {
            PolicyId = policyId;
        }
    }
}
