using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetAllCurrencies
{
    public class GetAllCurrenciesQuery : IRequest<Result<List<CurrencyDto>>>
    {
    }
}
