using IAMS.Application.DTOs.Currency;
using IAMS.Application.Features.Currencies.Queries.ConvertMoney;
using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.ConvertBatch
{
    public class ConvertBatchQueryHandler : IRequestHandler<ConvertBatchQuery, Result<List<CurrencyConversionDto>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConvertBatchQueryHandler> _logger;

        public ConvertBatchQueryHandler(
            IMediator mediator,
            ILogger<ConvertBatchQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<List<CurrencyConversionDto>>> Handle(ConvertBatchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var conversions = new List<CurrencyConversionDto>();

                foreach (var amount in request.Amounts)
                {
                    var convertResult = await _mediator.Send(new ConvertMoneyQuery(amount, request.ToCurrencyId), cancellationToken);
                    if (!convertResult.IsSuccess)
                    {
                        _logger.LogWarning("Failed to convert {Amount} {Currency}", amount.Amount, amount.CurrencyCode);
                        continue;
                    }

                    var rateResult = await _mediator.Send(new GetExchangeRateByIdsQuery(
                        amount.CurrencyId,
                        request.ToCurrencyId), cancellationToken);

                    var conversion = new CurrencyConversionDto
                    {
                        OriginalAmount = amount,
                        ConvertedAmount = convertResult.Data,
                        ExchangeRate = rateResult.IsSuccess ? rateResult.Data : 0,
                        ConversionDate = DateTime.UtcNow
                    };

                    conversions.Add(conversion);
                }

                return Result<List<CurrencyConversionDto>>.Success(conversions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing batch conversion to currency {To}", request.ToCurrencyId);
                return Result<List<CurrencyConversionDto>>.InternalError("Error performing batch conversion", new List<string> { ex.Message });
            }
        }
    }
}
