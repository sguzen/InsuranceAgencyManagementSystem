using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.ConvertAmountByIds
{
    public class ConvertAmountByIdsQuery : IRequest<Result<decimal>>
    {
        public decimal Amount { get; set; }
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public DateTime? Date { get; set; }

        public ConvertAmountByIdsQuery(decimal amount, int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            Amount = amount;
            FromCurrencyId = fromCurrencyId;
            ToCurrencyId = toCurrencyId;
            Date = date;
        }
    }
}
