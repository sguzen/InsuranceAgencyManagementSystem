using IAMS.Application.Features.Currencies.Queries.GetExchangeRateByCodes;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.ConvertAmountByCodes
{
    public class ConvertAmountByCodesQueryHandler : IRequestHandler<ConvertAmountByCodesQuery, Result<decimal>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConvertAmountByCodesQueryHandler> _logger;

        public ConvertAmountByCodesQueryHandler(
            IMediator mediator,
            ILogger<ConvertAmountByCodesQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(ConvertAmountByCodesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var rateResult = await _mediator.Send(new GetExchangeRateByCodesQuery(
                    request.FromCode,
                    request.ToCode,
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
                _logger.LogError(ex, "Error converting amount from {From} to {To}", request.FromCode, request.ToCode);
                return Result<decimal>.InternalError("Error converting amount", new List<string> { ex.Message });
            }
        }
    }
}
