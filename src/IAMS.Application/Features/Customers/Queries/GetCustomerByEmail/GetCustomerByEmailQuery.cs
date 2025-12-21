using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByEmail
{
    public record GetCustomerByEmailQuery(string Email) : IRequest<Result<CustomerDto>>;
}