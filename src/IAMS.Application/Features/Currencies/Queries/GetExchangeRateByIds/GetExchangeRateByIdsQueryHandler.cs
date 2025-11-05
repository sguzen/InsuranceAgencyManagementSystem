using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateByIds
{
    public class GetExchangeRateByIdsQueryHandler : IRequestHandler<GetExchangeRateByIdsQuery, Result<decimal>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetExchangeRateByIdsQueryHandler> _logger;

        public GetExchangeRateByIdsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetExchangeRateByIdsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(GetExchangeRateByIdsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // If same currency, rate is 1
                if (request.FromCurrencyId == request.ToCurrencyId)
                {
                    return Result<decimal>.Success(1.0m);
                }

                var targetDate = request.Date ?? DateTime.UtcNow;
                var exchangeRates = await _unitOfWork.CurrencyExchangeRates.GetAllAsync();

                var rate = exchangeRates
                    .Where(er => er.FromCurrencyId == request.FromCurrencyId
                                 && er.ToCurrencyId == request.ToCurrencyId
                                 && er.EffectiveDate <= targetDate
                                 && (er.ExpiryDate == null || er.ExpiryDate >= targetDate)
                                 && er.IsActive)
                    .OrderByDescending(er => er.EffectiveDate)
                    .FirstOrDefault();

                if (rate == null)
                {
                    // Try inverse rate
                    var inverseRate = exchangeRates
                        .Where(er => er.FromCurrencyId == request.ToCurrencyId
                                     && er.ToCurrencyId == request.FromCurrencyId
                                     && er.EffectiveDate <= targetDate
                                     && (er.ExpiryDate == null || er.ExpiryDate >= targetDate)
                                     && er.IsActive)
                        .OrderByDescending(er => er.EffectiveDate)
                        .FirstOrDefault();

                    if (inverseRate != null)
                    {
                        return Result<decimal>.Success(1 / inverseRate.Rate);
                    }

                    return Result<decimal>.NotFound($"Exchange rate not found for currencies {request.FromCurrencyId} to {request.ToCurrencyId}");
                }

                return Result<decimal>.Success(rate.Rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate from {From} to {To}", request.FromCurrencyId, request.ToCurrencyId);
                return Result<decimal>.InternalError("Error retrieving exchange rate", new List<string> { ex.Message });
            }
        }
    }
}
