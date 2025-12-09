using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Payments.Commands.DeletePayment
{
    public class DeletePaymentCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public DeletePaymentCommand(int id)
        {
            Id = id;
        }
    }
}
