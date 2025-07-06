using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByCode
{
    public record GetCustomerByCodeQuery(string CustomerCode) : IRequest<Result<CustomerDto>>;
}