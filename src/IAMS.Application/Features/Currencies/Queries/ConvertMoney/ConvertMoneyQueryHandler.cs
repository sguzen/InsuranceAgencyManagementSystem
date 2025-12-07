using IAMS.Application.DTOs.Currency;
using IAMS.Application.Features.Currencies.Queries.ConvertAmountByIds;
using IAMS.Application.Features.Currencies.Queries.GetCurrencyById;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.ConvertMoney
{
    public class ConvertMoneyQueryHandler : IRequestHandler<ConvertMoneyQuery, Result<MoneyDto>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConvertMoneyQueryHandler> _logger;

        public ConvertMoneyQueryHandler(
            IMediator mediator,
            ILogger<ConvertMoneyQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<MoneyDto>> Handle(ConvertMoneyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var convertResult = await _mediator.Send(new ConvertAmountByIdsQuery(
                    request.Money.Amount,
                    request.Money.CurrencyId,
                    request.ToCurrencyId,
                    request.Date), cancellationToken);

                if (!convertResult.IsSuccess)
                {
                    return Result<MoneyDto>.Failure(convertResult.Message, convertResult.Errors);
                }

                var toCurrencyResult = await _mediator.Send(new GetCurrencyByIdQuery(request.ToCurrencyId), cancellationToken);
                if (!toCurrencyResult.IsSuccess)
                {
                    return Result<MoneyDto>.Failure(toCurrencyResult.Message, toCurrencyResult.Errors);
                }

                var moneyDto = new MoneyDto
                {
                    Amount = convertResult.Data,
                    CurrencyId = request.ToCurrencyId,
                    CurrencyCode = toCurrencyResult.Data.Code,
                    CurrencySymbol = toCurrencyResult.Data.Symbol
                };

                return Result<MoneyDto>.Success(moneyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting money from currency {From} to {To}", request.Money.CurrencyId, request.ToCurrencyId);
                return Result<MoneyDto>.InternalError("Error converting money", new List<string> { ex.Message });
            }
        }
    }
}
