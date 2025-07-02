using MediatR;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommand : IRequest<Result<CustomerDto>>
    {
        public int Id { get; set; }
        public CreateOrUpdateCustomerDto CustomerDto { get; set; }

        public UpdateCustomerCommand(int id, CreateOrUpdateCustomerDto customerDto)
        {
            Id = id;
            CustomerDto = customerDto;
        }
    }
}