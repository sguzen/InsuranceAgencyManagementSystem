using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyByNumber
{
    public class GetPolicyByNumberQuery : IRequest<Result<PolicyDto>>
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public GetPolicyByNumberQuery(string policyNumber)
        {
            PolicyNumber = policyNumber;
        }

    }
}