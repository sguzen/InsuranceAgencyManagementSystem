using MediatR;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;

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