using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Queries.GetRecentPolicies
{
    public record GetRecentPoliciesQuery(int Count = 5) : IRequest<Result<List<PolicyDto>>>;
}