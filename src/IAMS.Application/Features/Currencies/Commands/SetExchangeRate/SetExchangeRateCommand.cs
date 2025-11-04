using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Commands.SetExchangeRate
{
    public class SetExchangeRateCommand : IRequest<Result<ExchangeRateDto>>
    {
        public CreateExchangeRateDto ExchangeRateDto { get; set; }

        public SetExchangeRateCommand(CreateExchangeRateDto exchangeRateDto)
        {
            ExchangeRateDto = exchangeRateDto;
        }
    }
}
