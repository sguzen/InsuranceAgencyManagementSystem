using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Features.Currencies.Commands.SetExchangeRate;
using IAMS.Application.Features.Currencies.Commands.SetExchangeRates;
using IAMS.Application.Features.Currencies.Commands.UpdateExchangeRatesFromTCMB;
using IAMS.Application.Features.Currencies.Queries.ConvertAmountByCodes;
using IAMS.Application.Features.Currencies.Queries.ConvertAmountByIds;
using IAMS.Application.Features.Currencies.Queries.ConvertBatch;
using IAMS.Application.Features.Currencies.Queries.ConvertMoney;
using IAMS.Application.Features.Currencies.Queries.ConvertToBaseCurrency;
using IAMS.Application.Features.Currencies.Queries.GetActiveCurrencies;
using IAMS.Application.Features.Currencies.Queries.GetAllCurrencies;
using IAMS.Application.Features.Currencies.Queries.GetAllExchangeRatesToBase;
using IAMS.Application.Features.Currencies.Queries.GetBaseCurrency;
using IAMS.Application.Features.Currencies.Queries.GetCurrencyByCode;
using IAMS.Application.Features.Currencies.Queries.GetCurrencyById;
using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByCodes;
using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds;
using IAMS.Application.Features.Currencies.Queries.GetExchangeRateHistory;
using IAMS.Application.Features.Currencies.Queries.GetLatestExchangeRate;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Services.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMediator _mediator;

        public CurrencyService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Currency Management
        public async Task<Result<List<CurrencyDto>>> GetAllCurrenciesAsync()
        {
            return await _mediator.Send(new GetAllCurrenciesQuery());
        }

        public async Task<Result<List<CurrencyDto>>> GetActiveCurrenciesAsync()
        {
            return await _mediator.Send(new GetActiveCurrenciesQuery());
        }

        public async Task<Result<CurrencyDto>> GetByIdAsync(int id)
        {
            return await _mediator.Send(new GetCurrencyByIdQuery(id));
        }

        public async Task<Result<CurrencyDto>> GetByCodeAsync(string code)
        {
            return await _mediator.Send(new GetCurrencyByCodeQuery(code));
        }

        public async Task<Result<CurrencyDto>> GetBaseCurrencyAsync()
        {
            return await _mediator.Send(new GetBaseCurrencyQuery());
        }

        // Exchange Rates
        public async Task<Result<decimal>> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            return await _mediator.Send(new GetExchangeRateByIdsQuery(fromCurrencyId, toCurrencyId, date));
        }

        public async Task<Result<decimal>> GetExchangeRateAsync(string fromCode, string toCode, DateTime? date = null)
        {
            return await _mediator.Send(new GetExchangeRateByCodesQuery(fromCode, toCode, date));
        }

        public async Task<Result<ExchangeRateDto>> GetLatestExchangeRateAsync(int fromCurrencyId, int toCurrencyId)
        {
            return await _mediator.Send(new GetLatestExchangeRateQuery(fromCurrencyId, toCurrencyId));
        }

        public async Task<Result<List<ExchangeRateDto>>> GetExchangeRateHistoryAsync(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate)
        {
            return await _mediator.Send(new GetExchangeRateHistoryQuery(fromCurrencyId, toCurrencyId, startDate, endDate));
        }

        // Set Exchange Rates
        public async Task<Result<ExchangeRateDto>> SetExchangeRateAsync(CreateExchangeRateDto dto)
        {
            return await _mediator.Send(new SetExchangeRateCommand(dto));
        }

        public async Task<Result<List<ExchangeRateDto>>> SetExchangeRatesAsync(List<CreateExchangeRateDto> rates)
        {
            return await _mediator.Send(new SetExchangeRatesCommand(rates));
        }

        public async Task<Result> UpdateExchangeRatesFromTCMBAsync()
        {
            return await _mediator.Send(new UpdateExchangeRatesFromTCMBCommand());
        }

        // Currency Conversion
        public async Task<Result<decimal>> ConvertAmountAsync(decimal amount, int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            return await _mediator.Send(new ConvertAmountByIdsQuery(amount, fromCurrencyId, toCurrencyId, date));
        }

        public async Task<Result<decimal>> ConvertAmountAsync(decimal amount, string fromCode, string toCode, DateTime? date = null)
        {
            return await _mediator.Send(new ConvertAmountByCodesQuery(amount, fromCode, toCode, date));
        }

        public async Task<Result<decimal>> ConvertToBaseCurrencyAsync(decimal amount, int fromCurrencyId, DateTime? date = null)
        {
            return await _mediator.Send(new ConvertToBaseCurrencyQuery(amount, fromCurrencyId, date));
        }

        public async Task<Result<MoneyDto>> ConvertMoney(MoneyDto money, int toCurrencyId, DateTime? date = null)
        {
            return await _mediator.Send(new ConvertMoneyQuery(money, toCurrencyId, date));
        }

        // Batch Operations
        public async Task<Result<Dictionary<string, decimal>>> GetAllExchangeRatesToBaseAsync(DateTime? date = null)
        {
            return await _mediator.Send(new GetAllExchangeRatesToBaseQuery(date));
        }

        public async Task<Result<List<CurrencyConversionDto>>> ConvertBatchAsync(List<MoneyDto> amounts, int toCurrencyId)
        {
            return await _mediator.Send(new ConvertBatchQuery(amounts, toCurrencyId));
        }
    }
}
