using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

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