using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithActivePolicies
{
    public record GetCustomersWithActivePoliciesQuery() : IRequest<Result<List<CustomerDto>>>;
}