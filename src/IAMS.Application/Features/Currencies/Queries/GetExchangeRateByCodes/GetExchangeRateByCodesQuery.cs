using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateByCodes
{
    public class GetExchangeRateByCodesQuery : IRequest<Result<decimal>>
    {
        public string FromCode { get; set; }
        public string ToCode { get; set; }
        public DateTime? Date { get; set; }

        public GetExchangeRateByCodesQuery(string fromCode, string toCode, DateTime? date = null)
        {
            FromCode = fromCode;
            ToCode = toCode;
            Date = date;
        }
    }
}
