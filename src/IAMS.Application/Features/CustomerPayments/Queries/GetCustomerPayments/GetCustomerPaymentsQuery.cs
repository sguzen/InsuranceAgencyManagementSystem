using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.CustomerPayments.Queries.GetCustomerPayments
{
    public record GetCustomerPaymentsQuery(int CustomerId)
        : IRequest<Result<List<CustomerPaymentDto>>>;
}
