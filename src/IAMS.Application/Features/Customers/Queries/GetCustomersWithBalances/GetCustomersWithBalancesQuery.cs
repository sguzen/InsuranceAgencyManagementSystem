using MediatR;
using IAMS.Shared.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithBalances
{
    public class GetCustomersWithBalancesQuery : IRequest<Result<List<CustomerWithBalanceDto>>>
    {
    }
}
