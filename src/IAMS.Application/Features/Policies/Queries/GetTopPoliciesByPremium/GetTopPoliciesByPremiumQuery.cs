using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetTopPoliciesByPremium
{
    public class GetTopPoliciesByPremiumQuery : IRequest<Result<List<PolicyDto>>>
    {
        public int Count { get; }

        public GetTopPoliciesByPremiumQuery(int count = 10)
        {
            Count = count;
        }
    }
}