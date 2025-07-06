using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyByNumber
{
    public record GetPolicyByNumberQuery(string PolicyNumber) : IRequest<Result<PolicyDto>>;
}