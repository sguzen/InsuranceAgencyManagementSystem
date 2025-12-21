using IAMS.Shared.DTOs.Payment;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommand : IRequest<Result>
    {
        public UpdatePolicyPaymentDto PaymentDto { get; set; }

        public UpdatePaymentCommand(UpdatePolicyPaymentDto paymentDto)
        {
            PaymentDto = paymentDto;
        }
    }
}
