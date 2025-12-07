using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount
{
    public record GetTotalCustomersCountQuery() : IRequest<Result<int>>;
}