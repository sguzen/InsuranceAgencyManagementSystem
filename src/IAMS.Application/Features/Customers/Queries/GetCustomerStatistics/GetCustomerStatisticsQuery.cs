using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerStatistics
{
    public record GetCustomerStatisticsQuery() : IRequest<Result<CustomerStatisticsDto>>;
}