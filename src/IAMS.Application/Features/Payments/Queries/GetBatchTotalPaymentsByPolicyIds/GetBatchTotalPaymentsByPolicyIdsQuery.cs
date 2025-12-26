using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetBatchTotalPaymentsByPolicyIds
{
    public class GetBatchTotalPaymentsByPolicyIdsQuery : IRequest<Result<Dictionary<int, decimal>>>
    {
        public List<int> PolicyIds { get; set; }

        public GetBatchTotalPaymentsByPolicyIdsQuery(List<int> policyIds)
        {
            PolicyIds = policyIds ?? new List<int>();
        }
    }
}
