using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.ConvertToBaseCurrency
{
    public class ConvertToBaseCurrencyQuery : IRequest<Result<decimal>>
    {
        public decimal Amount { get; set; }
        public int FromCurrencyId { get; set; }
        public DateTime? Date { get; set; }

        public ConvertToBaseCurrencyQuery(decimal amount, int fromCurrencyId, DateTime? date = null)
        {
            Amount = amount;
            FromCurrencyId = fromCurrencyId;
            Date = date;
        }
    }
}
