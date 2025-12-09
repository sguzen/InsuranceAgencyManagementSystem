using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Queries.GetEndorsementsByPolicyId
{
    public record GetEndorsementsByPolicyIdQuery(int PolicyId) : IRequest<Result<List<PolicyDto>>>;
}
