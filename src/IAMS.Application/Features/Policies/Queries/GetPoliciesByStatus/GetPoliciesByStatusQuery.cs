using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus
{
    public record GetPoliciesByStatusQuery() : IRequest<Result<Dictionary<PolicyStatus, int>>>;
}