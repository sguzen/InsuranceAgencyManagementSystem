using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetActiveCurrencies
{
    public class GetActiveCurrenciesQuery : IRequest<Result<List<CurrencyDto>>>
    {
    }
}
