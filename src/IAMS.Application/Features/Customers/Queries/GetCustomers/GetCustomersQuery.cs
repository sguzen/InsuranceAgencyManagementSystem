using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQuery : IRequest<Result<PagedResult<CustomerDto>>>
    {
        public CustomerQueryParams QueryParams { get; set; }

        public GetCustomersQuery(CustomerQueryParams queryParams)
        {
            QueryParams = queryParams;
        }
    }
}