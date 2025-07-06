using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyStatistics
{
    public record GetPolicyStatisticsQuery() : IRequest<Result<PolicyStatisticsDto>>;
}