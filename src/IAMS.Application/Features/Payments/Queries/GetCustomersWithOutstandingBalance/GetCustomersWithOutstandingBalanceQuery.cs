using IAMS.Application.DTOs.Payment;
using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetCustomersWithOutstandingBalance
{
    public class GetCustomersWithOutstandingBalanceQuery : IRequest<List<CustomerOutstandingBalanceDto>>
    {
    }
}
