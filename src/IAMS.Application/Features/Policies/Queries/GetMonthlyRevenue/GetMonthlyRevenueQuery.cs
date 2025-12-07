using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue
{
    public class GetMonthlyRevenueQuery : IRequest<Result<decimal>>
    {
        public DateTime? Month { get; }

        public GetMonthlyRevenueQuery(DateTime? month = null)
        {
            Month = month ?? DateTime.Now;
        }
    }
}