using IAMS.Shared.DTOs.Claim;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Claims.Queries.GetClaimsPaged
{
    public class GetClaimsPagedQuery : IRequest<PagedResult<PolicyClaimDto>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }

        public GetClaimsPagedQuery(int pageNumber, int pageSize, string? searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;
        }
    }
}
