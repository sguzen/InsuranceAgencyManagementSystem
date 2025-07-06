using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerStatistics
{
    public record GetCustomerStatisticsQuery() : IRequest<Result<CustomerStatisticsDto>>;
}