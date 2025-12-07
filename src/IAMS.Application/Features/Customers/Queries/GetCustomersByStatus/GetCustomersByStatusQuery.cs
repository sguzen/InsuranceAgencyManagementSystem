using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersByStatus
{
    public record GetCustomersByStatusQuery() : IRequest<Result<Dictionary<CustomerStatus, int>>>;
}