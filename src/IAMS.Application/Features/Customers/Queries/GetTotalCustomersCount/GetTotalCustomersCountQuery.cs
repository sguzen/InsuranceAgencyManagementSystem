using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount
{
    public record GetTotalCustomersCountQuery(int TenantId) : IRequest<Result<int>>;
}