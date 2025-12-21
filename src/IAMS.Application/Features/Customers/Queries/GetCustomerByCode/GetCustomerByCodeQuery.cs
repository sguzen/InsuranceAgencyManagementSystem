using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Shared.DTOs.Customer;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByCode
{
    public record GetCustomerByCodeQuery(string CustomerCode) : IRequest<Result<CustomerDto>>;
}