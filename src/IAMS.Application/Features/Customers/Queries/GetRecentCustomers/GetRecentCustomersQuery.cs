using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetRecentCustomers
{
    public record GetRecentCustomersQuery(int Count = 5) : IRequest<Result<List<CustomerDto>>>;
}