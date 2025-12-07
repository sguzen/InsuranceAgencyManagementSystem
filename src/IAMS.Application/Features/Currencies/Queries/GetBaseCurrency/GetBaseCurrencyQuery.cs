using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetBaseCurrency
{
    public class GetBaseCurrencyQuery : IRequest<Result<CurrencyDto>>
    {
    }
}
