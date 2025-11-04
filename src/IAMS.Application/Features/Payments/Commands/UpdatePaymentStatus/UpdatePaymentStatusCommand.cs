using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public class UpdatePaymentStatusCommand : IRequest<Result>
    {
        public int Id { get; set; }
        public PaymentStatus Status { get; set; }

        public UpdatePaymentStatusCommand(int id, PaymentStatus status)
        {
            Id = id;
            Status = status;
        }
    }
}
