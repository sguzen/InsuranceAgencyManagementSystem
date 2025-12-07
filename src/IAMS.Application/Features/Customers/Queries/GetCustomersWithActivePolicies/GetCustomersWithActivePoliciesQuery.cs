using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithActivePolicies
{
    public record GetCustomersWithActivePoliciesQuery() : IRequest<Result<List<CustomerDto>>>;
}