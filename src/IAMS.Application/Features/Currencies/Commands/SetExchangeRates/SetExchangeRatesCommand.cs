using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Currencies.Commands.SetExchangeRates
{
    public class SetExchangeRatesCommand : IRequest<Result<List<ExchangeRateDto>>>
    {
        public List<CreateExchangeRateDto> ExchangeRates { get; set; }

        public SetExchangeRatesCommand(List<CreateExchangeRateDto> exchangeRates)
        {
            ExchangeRates = exchangeRates;
        }
    }
}
