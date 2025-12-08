using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

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