using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds
{
    public class GetExchangeRateByIdsQuery : IRequest<Result<decimal>>
    {
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public DateTime? Date { get; set; }

        public GetExchangeRateByIdsQuery(int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
            Date = date;
        }
    }
}
