using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsPaged
{
    public class GetPaymentsPagedQuery : IRequest<PagedResult<PolicyPaymentDto>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }

        public GetPaymentsPagedQuery(int pageNumber, int pageSize, string? searchTerm = null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;
        }
    }
}
