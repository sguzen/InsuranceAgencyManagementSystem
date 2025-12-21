using IAMS.Shared.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentById
{
    public class GetPaymentByIdQuery : IRequest<PolicyPaymentDto?>
    {
        public int Id { get; set; }

        public GetPaymentByIdQuery(int id)
        {
            Id = id;
        }
    }
}
