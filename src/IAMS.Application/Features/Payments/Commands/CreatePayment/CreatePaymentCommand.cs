using IAMS.Application.DTOs.Payment;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommand : IRequest<Result<PolicyPaymentDto>>
    {
        public CreatePolicyPaymentDto PaymentDto { get; set; }

        public CreatePaymentCommand(CreatePolicyPaymentDto paymentDto)
        {
            PaymentDto = paymentDto;
        }
    }
}
