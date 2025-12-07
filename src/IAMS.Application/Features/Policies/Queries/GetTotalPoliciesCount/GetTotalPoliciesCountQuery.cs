using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetTotalPoliciesCount
{
    public record GetTotalPoliciesCountQuery() : IRequest<Result<int>>;
}