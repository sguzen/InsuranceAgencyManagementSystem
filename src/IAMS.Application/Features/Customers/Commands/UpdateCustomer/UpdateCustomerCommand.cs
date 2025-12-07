using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Commands.UpdateCustomer
{
    public record UpdateCustomerCommand(int Id, CreateOrUpdateCustomerDto CustomerDto) : IRequest<Result<CustomerDto>>;
}