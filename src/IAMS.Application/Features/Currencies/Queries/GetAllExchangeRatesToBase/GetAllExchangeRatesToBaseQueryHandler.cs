using IAMS.Application.Features.Currencies.Queries.GetActiveCurrencies;
using IAMS.Application.Features.Currencies.Queries.GetBaseCurrency;
using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetAllExchangeRatesToBase
{
    public class GetAllExchangeRatesToBaseQueryHandler : IRequestHandler<GetAllExchangeRatesToBaseQuery, Result<Dictionary<string, decimal>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllExchangeRatesToBaseQueryHandler> _logger;

        public GetAllExchangeRatesToBaseQueryHandler(
            IMediator mediator,
            ILogger<GetAllExchangeRatesToBaseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, decimal>>> Handle(GetAllExchangeRatesToBaseQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var baseCurrencyResult = await _mediator.Send(new GetBaseCurrencyQuery(), cancellationToken);
                if (!baseCurrencyResult.IsSuccess)
                {
                    return Result<Dictionary<string, decimal>>.Failure(baseCurrencyResult.Message, baseCurrencyResult.Errors);
                }

                var currenciesResult = await _mediator.Send(new GetActiveCurrenciesQuery(), cancellationToken);
                if (!currenciesResult.IsSuccess)
                {
                    return Result<Dictionary<string, decimal>>.Failure(currenciesResult.Message, currenciesResult.Errors);
                }

                var rates = new Dictionary<string, decimal>();
                var baseCurrency = baseCurrencyResult.Data;

                foreach (var currency in currenciesResult.Data)
                {
                    if (currency.Id == baseCurrency.Id)
                    {
                        rates[currency.Code] = 1.0m;
                    }
                    else
                    {
                        var rateResult = await _mediator.Send(new GetExchangeRateByIdsQuery(
                            currency.Id,
                            baseCurrency.Id,
                            request.Date), cancellationToken);

                        if (rateResult.IsSuccess)
                        {
                            rates[currency.Code] = rateResult.Data;
                        }
                        else
                        {
                            _logger.LogWarning("Could not get exchange rate for {Code} to base currency", currency.Code);
                        }
                    }
                }

                return Result<Dictionary<string, decimal>>.Success(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all exchange rates to base currency");
                return Result<Dictionary<string, decimal>>.InternalError("Error retrieving exchange rates", new List<string> { ex.Message });
            }
        }
    }
}
