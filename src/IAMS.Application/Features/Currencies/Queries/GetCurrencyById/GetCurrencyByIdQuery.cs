using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQuery : IRequest<Result<CurrencyDto>>
    {
        public int Id { get; set; }

        public GetCurrencyByIdQuery(int id)
        {
            Id = id;
        }
    }
}
