using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetRevenueByMonth
{
    public record GetRevenueByMonthQuery(int Months = 12) : IRequest<Result<Dictionary<string, decimal>>>;
}