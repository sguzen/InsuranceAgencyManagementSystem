using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByCustomer
{
    public record GetPoliciesByCustomerQuery(int CustomerId) : IRequest<Result<List<PolicyDto>>>;
}