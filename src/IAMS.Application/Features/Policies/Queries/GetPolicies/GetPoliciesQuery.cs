using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesQuery : IRequest<Result<PagedResult<PolicyDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? CustomerId { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public PolicyStatus? Status { get; set; }
    }
}