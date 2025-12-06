using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetEndorsementsByPolicyId
{
    public record GetEndorsementsByPolicyIdQuery(int PolicyId) : IRequest<Result<List<PolicyDto>>>;
}
