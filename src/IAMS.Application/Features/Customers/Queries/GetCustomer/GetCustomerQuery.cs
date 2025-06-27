using MediatR;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;

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