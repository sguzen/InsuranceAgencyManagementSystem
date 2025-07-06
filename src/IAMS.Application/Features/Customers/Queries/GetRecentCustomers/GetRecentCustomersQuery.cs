using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetRecentCustomers
{
    public record GetRecentCustomersQuery(int Count = 5) : IRequest<Result<List<CustomerDto>>>;
}