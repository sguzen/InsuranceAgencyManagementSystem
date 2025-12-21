using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateHistory
{
    public class GetExchangeRateHistoryQuery : IRequest<Result<List<ExchangeRateDto>>>
    {
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public GetExchangeRateHistoryQuery(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate)
        {
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
