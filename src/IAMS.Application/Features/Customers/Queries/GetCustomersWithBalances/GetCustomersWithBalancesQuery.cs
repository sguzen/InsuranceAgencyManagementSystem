using MediatR;
using IAMS.Shared.Models;
using IAMS.Shared.DTOs.Customer;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithBalances
{
    public class GetCustomersWithBalancesQuery : IRequest<Result<List<CustomerWithBalanceDto>>>
    {
        /// <summary>
        /// When set, restricts the aggregation to a single customer (e.g. the policy
        /// details page only needs that customer's balances).
        /// </summary>
        public int? CustomerId { get; set; }

        public GetCustomersWithBalancesQuery(int? customerId = null)
        {
            CustomerId = customerId;
        }
    }
}
