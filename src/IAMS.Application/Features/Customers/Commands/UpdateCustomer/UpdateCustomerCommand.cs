using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public record UpdateCustomerCommand(int Id, CreateOrUpdateCustomerDto CustomerDto) : IRequest<Result<CustomerDto>>;
}