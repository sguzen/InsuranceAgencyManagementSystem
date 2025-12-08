using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using MediatR;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus
{
    public record GetPoliciesByStatusQuery() : IRequest<Result<Dictionary<PolicyStatus, int>>>;
}