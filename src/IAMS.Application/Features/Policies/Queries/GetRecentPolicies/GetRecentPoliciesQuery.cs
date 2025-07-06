using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetRecentPolicies
{
    public record GetRecentPoliciesQuery(int Count = 5) : IRequest<Result<List<PolicyDto>>>;
}