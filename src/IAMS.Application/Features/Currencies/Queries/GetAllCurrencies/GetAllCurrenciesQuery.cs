using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetAllCurrencies
{
    public class GetAllCurrenciesQuery : IRequest<Result<List<CurrencyDto>>>
    {
    }
}
