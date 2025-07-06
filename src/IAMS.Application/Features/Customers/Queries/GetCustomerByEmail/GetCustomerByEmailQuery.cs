using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByEmail
{
    public record GetCustomerByEmailQuery(string Email) : IRequest<Result<CustomerDto>>;
}