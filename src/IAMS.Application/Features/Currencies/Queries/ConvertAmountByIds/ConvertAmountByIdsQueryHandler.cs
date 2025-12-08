using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.ConvertAmountByIds
{
    public class ConvertAmountByIdsQueryHandler : IRequestHandler<ConvertAmountByIdsQuery, Result<decimal>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConvertAmountByIdsQueryHandler> _logger;

        public ConvertAmountByIdsQueryHandler(
            IMediator mediator,
            ILogger<ConvertAmountByIdsQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(ConvertAmountByIdsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var rateResult = await _mediator.Send(new GetExchangeRateByIdsQuery(
                    request.FromCurrencyId,
                    request.ToCurrencyId,
                    request.Date), cancellationToken);

                if (!rateResult.IsSuccess)
                {
                    return Result<decimal>.Failure(rateResult.Message, rateResult.Errors);
                }

                var convertedAmount = request.Amount * rateResult.Data;
                return Result<decimal>.Success(convertedAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount from currency {From} to {To}", request.FromCurrencyId, request.ToCurrencyId);
                return Result<decimal>.InternalError("Error converting amount", new List<string> { ex.Message });
            }
        }
    }
}
