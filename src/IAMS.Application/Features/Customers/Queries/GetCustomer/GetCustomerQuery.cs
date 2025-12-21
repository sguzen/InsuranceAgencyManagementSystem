using MediatR;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomer
{
    public class GetCustomerQuery : IRequest<Result<CustomerDto>>
    {
        public int Id { get; set; }

        public GetCustomerQuery(int id)
        {
            Id = id;
        }
    }
}