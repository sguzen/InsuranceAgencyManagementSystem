using IAMS.Application.DTOs.Currency;
using IAMS.Application.Features.Currencies.Commands.SetExchangeRate;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Commands.SetExchangeRates
{
    public class SetExchangeRatesCommandHandler : IRequestHandler<SetExchangeRatesCommand, Result<List<ExchangeRateDto>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SetExchangeRatesCommandHandler> _logger;

        public SetExchangeRatesCommandHandler(
            IMediator mediator,
            ILogger<SetExchangeRatesCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<List<ExchangeRateDto>>> Handle(SetExchangeRatesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<ExchangeRateDto>();
                var errors = new List<string>();

                foreach (var rateDto in request.ExchangeRates)
                {
                    var result = await _mediator.Send(new SetExchangeRateCommand(rateDto), cancellationToken);
                    if (result.IsSuccess)
                    {
                        results.Add(result.Data);
                    }
                    else
                    {
                        errors.Add($"Failed to set rate from {rateDto.FromCurrencyId} to {rateDto.ToCurrencyId}: {result.Message}");
                        _logger.LogWarning("Failed to set exchange rate: {Message}", result.Message);
                    }
                }

                if (results.Count == 0)
                {
                    return Result<List<ExchangeRateDto>>.Failure("Failed to set any exchange rates", errors);
                }

                _logger.LogInformation("Set {Count} exchange rates successfully", results.Count);
                return Result<List<ExchangeRateDto>>.Success(results, $"Successfully set {results.Count} exchange rates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting exchange rates");
                return Result<List<ExchangeRateDto>>.InternalError("Error setting exchange rates", new List<string> { ex.Message });
            }
        }
    }
}
