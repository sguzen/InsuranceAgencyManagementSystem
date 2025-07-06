using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount
{
    public record GetExpiringPoliciesCountQuery(int DaysAhead = 30) : IRequest<Result<int>>;
}