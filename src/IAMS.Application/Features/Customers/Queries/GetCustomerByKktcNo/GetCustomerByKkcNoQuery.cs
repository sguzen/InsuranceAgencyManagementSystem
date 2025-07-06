using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByKktcNo
{
    public record GetCustomerByKktcNoQuery(string KkcNo) : IRequest<Result<CustomerDto>>;
}