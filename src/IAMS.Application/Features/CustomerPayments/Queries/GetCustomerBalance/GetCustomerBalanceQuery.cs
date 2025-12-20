using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.CustomerPayments.Queries.GetCustomerBalance
{
    public record GetCustomerBalanceQuery(int CustomerId)
        : IRequest<Result<CustomerBalanceDto>>;
}
