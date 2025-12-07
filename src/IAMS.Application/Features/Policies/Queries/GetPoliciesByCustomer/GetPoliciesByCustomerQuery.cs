using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer
{
    public record GetPoliciesByCustomerQuery(int CustomerId) : IRequest<Result<List<PolicyDto>>>;
}