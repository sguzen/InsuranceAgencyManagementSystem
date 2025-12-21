using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.CustomerPayments.Commands.CreateCustomerPayment
{
    public record CreateCustomerPaymentCommand(CreateCustomerPaymentDto Payment)
        : IRequest<Result<CustomerPaymentDto>>;
}
