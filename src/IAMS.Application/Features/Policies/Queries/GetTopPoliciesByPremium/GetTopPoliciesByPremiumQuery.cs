using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

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