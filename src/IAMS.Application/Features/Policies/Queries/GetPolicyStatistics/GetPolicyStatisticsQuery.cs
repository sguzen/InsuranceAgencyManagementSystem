using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyStatistics
{
    public record GetPolicyStatisticsQuery() : IRequest<Result<PolicyStatisticsDto>>;
}