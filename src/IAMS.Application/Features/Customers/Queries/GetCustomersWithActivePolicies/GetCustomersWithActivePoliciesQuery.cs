using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithActivePolicies
{
    public record GetCustomersWithActivePoliciesQuery() : IRequest<Result<List<CustomerDto>>>;
}