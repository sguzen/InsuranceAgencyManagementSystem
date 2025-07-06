using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetTopCustomersByPolicyCount
{
    public record GetTopCustomersByPolicyCountQuery(int Count = 10) : IRequest<Result<List<CustomerDto>>>;
}