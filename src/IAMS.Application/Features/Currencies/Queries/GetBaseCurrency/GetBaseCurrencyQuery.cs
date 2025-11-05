using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetBaseCurrency
{
    public class GetBaseCurrencyQuery : IRequest<Result<CurrencyDto>>
    {
    }
}
