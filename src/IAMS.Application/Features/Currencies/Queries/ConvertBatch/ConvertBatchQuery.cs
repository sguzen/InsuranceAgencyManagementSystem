using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.ConvertBatch
{
    public class ConvertBatchQuery : IRequest<Result<List<CurrencyConversionDto>>>
    {
        public List<MoneyDto> Amounts { get; set; }
        public int ToCurrencyId { get; set; }

        public ConvertBatchQuery(List<MoneyDto> amounts, int toCurrencyId)
        {
            Amounts = amounts;
            ToCurrencyId = toCurrencyId;
        }
    }
}
