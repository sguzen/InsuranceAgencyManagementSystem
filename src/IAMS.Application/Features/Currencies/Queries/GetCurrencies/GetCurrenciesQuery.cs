using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencies
{
    public record GetCurrenciesQuery : IRequest<Result<List<CurrencyDto>>>;
}
