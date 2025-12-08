using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.ConvertAmountByCodes
{
    public class ConvertAmountByCodesQuery : IRequest<Result<decimal>>
    {
        public decimal Amount { get; set; }
        public string FromCode { get; set; }
        public string ToCode { get; set; }
        public DateTime? Date { get; set; }

        public ConvertAmountByCodesQuery(decimal amount, string fromCode, string toCode, DateTime? date = null)
        {
            Amount = amount;
            FromCode = fromCode;
            ToCode = toCode;
            Date = date;
        }
    }
}
