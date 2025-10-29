using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo
{
    public record GetCustomerByIdentificationNoQuery(string IdentificationNo, int TentantId) : IRequest<Result<CustomerDto>>;
}