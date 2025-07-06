using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue
{
    public record GetMonthlyRevenueQuery() : IRequest<Result<decimal>>;
}