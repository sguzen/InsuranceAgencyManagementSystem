using MediatR;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

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