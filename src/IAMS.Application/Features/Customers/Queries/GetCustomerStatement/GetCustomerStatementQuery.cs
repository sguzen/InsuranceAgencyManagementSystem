using IAMS.Shared.DTOs.Customer;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerStatement
{
    /// <summary>
    /// Query to get multi-currency customer statement
    /// </summary>
    public record GetCustomerStatementQuery(
        int CustomerId,
        DateTime StartDate,
        DateTime EndDate,
        string? CurrencyCode = null  // Optional: filter by specific currency
    ) : IRequest<Result<CustomerMultiCurrencyStatementDto>>;
}
