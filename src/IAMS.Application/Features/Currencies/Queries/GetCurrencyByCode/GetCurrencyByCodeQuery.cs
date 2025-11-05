using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencyByCode
{
    public class GetCurrencyByCodeQuery : IRequest<Result<CurrencyDto>>
    {
        public string Code { get; set; }

        public GetCurrencyByCodeQuery(string code)
        {
            Code = code;
        }
    }
}
