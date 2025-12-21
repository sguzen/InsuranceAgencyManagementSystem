using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetTopCustomersByPolicyCount
{
    public record GetTopCustomersByPolicyCountQuery(int Count = 10) : IRequest<Result<List<CustomerDto>>>;
}