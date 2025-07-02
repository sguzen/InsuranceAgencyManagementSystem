using MediatR;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommand : IRequest<Result<CustomerDto>>
    {
        public CreateOrUpdateCustomerDto CustomerDto { get; set; }

        public CreateCustomerCommand(CreateOrUpdateCustomerDto customerDto)
        {
            CustomerDto = customerDto;
        }
    }
}