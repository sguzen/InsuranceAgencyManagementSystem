using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByCode
{
    public record GetCustomerByCodeQuery(string CustomerCode) : IRequest<Result<CustomerDto>>;
}