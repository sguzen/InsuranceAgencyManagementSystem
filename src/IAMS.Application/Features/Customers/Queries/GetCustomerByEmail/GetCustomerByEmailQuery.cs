using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByEmail
{
    public record GetCustomerByEmailQuery(string Email) : IRequest<Result<CustomerDto>>;
}