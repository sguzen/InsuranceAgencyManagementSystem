using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetTopPoliciesByPremium
{
    public record GetTopPoliciesByPremiumQuery(int Count = 10) : IRequest<Result<List<PolicyDto>>>;
}