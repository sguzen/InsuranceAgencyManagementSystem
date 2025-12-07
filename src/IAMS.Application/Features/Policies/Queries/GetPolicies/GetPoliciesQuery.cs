using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;

namespace IAMS.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesQuery : IRequest<Result<PagedResult<PolicyDto>>>
    {
        public PolicyQueryParams QueryParams { get; set; }

        public GetPoliciesQuery(PolicyQueryParams queryParams)
        {
            QueryParams = queryParams;
        }
    }
}