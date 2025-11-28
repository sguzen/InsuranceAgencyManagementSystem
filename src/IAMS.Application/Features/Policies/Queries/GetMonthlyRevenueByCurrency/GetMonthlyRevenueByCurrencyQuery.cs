using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenueByCurrency
{
    public class GetMonthlyRevenueByCurrencyQuery : IRequest<Result<Dictionary<string, decimal>>>
    {
        public DateTime? Month { get; }

        public GetMonthlyRevenueByCurrencyQuery(DateTime? month = null)
        {
            Month = month ?? DateTime.Now;
        }
    }
}
