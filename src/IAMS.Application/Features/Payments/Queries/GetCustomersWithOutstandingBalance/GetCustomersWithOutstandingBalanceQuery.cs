using IAMS.Shared.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetCustomersWithOutstandingBalance
{
    public class GetCustomersWithOutstandingBalanceQuery : IRequest<List<CustomerOutstandingBalanceDto>>
    {
        public int? Limit { get; set; }

        public GetCustomersWithOutstandingBalanceQuery(int? limit = null)
        {
            Limit = limit;
        }
    }
}
