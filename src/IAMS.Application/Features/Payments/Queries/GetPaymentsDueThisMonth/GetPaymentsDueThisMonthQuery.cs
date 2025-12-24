using IAMS.Shared.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetPaymentsDueThisMonth
{
    public class GetPaymentsDueThisMonthQuery : IRequest<List<PolicyPaymentDto>>
    {
        public int? Limit { get; set; }
    }
}
