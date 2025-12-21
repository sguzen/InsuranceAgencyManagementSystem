using IAMS.Shared.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetOverduePayments
{
    public class GetOverduePaymentsQuery : IRequest<List<PolicyPaymentDto>>
    {
    }
}
