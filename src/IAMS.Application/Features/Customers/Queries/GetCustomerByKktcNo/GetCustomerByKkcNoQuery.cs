using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerByIdentificationNo
{
    public record GetCustomerByIdentificationNoQuery(string IdentificationNo) : IRequest<Result<CustomerDto>>;
}