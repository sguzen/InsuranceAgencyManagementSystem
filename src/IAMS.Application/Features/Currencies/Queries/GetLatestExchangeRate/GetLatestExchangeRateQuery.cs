using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetLatestExchangeRate
{
    public class GetLatestExchangeRateQuery : IRequest<Result<ExchangeRateDto>>
    {
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }

        public GetLatestExchangeRateQuery(int fromCurrencyId, int toCurrencyId)
        {
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
        }
    }
}
