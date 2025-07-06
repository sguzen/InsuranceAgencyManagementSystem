using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersCreatedByMonth
{
    public record GetCustomersCreatedByMonthQuery(int Months = 12) : IRequest<Result<Dictionary<string, int>>>;
}