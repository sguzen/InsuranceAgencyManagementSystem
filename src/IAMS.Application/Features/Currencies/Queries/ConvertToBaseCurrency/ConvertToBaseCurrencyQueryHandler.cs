using IAMS.Application.Features.Currencies.Queries.ConvertAmountByIds;
using IAMS.Application.Features.Currencies.Queries.GetBaseCurrency;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.ConvertToBaseCurrency
{
    public class ConvertToBaseCurrencyQueryHandler : IRequestHandler<ConvertToBaseCurrencyQuery, Result<decimal>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConvertToBaseCurrencyQueryHandler> _logger;

        public ConvertToBaseCurrencyQueryHandler(
            IMediator mediator,
            ILogger<ConvertToBaseCurrencyQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(ConvertToBaseCurrencyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var baseCurrencyResult = await _mediator.Send(new GetBaseCurrencyQuery(), cancellationToken);
                if (!baseCurrencyResult.IsSuccess)
                {
                    return Result<decimal>.Failure(baseCurrencyResult.Message, baseCurrencyResult.Errors);
                }

                var convertResult = await _mediator.Send(new ConvertAmountByIdsQuery(
                    request.Amount,
                    request.FromCurrencyId,
                    baseCurrencyResult.Data.Id,
                    request.Date), cancellationToken);

                return convertResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount to base currency from currency {From}", request.FromCurrencyId);
                return Result<decimal>.InternalError("Error converting to base currency", new List<string> { ex.Message });
            }
        }
    }
}
