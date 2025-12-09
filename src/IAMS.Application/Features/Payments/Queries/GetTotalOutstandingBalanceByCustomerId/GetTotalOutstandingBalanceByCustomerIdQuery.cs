using MediatR;

namespace IAMS.Application.Features.Payments.Queries.GetTotalOutstandingBalanceByCustomerId
{
    public class GetTotalOutstandingBalanceByCustomerIdQuery : IRequest<decimal>
    {
        public int CustomerId { get; set; }

        public GetTotalOutstandingBalanceByCustomerIdQuery(int customerId)
        {
            CustomerId = customerId;
        }
    }
}
