using AutoMapper;
using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetLatestExchangeRate
{
    public class GetLatestExchangeRateQueryHandler : IRequestHandler<GetLatestExchangeRateQuery, Result<ExchangeRateDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetLatestExchangeRateQueryHandler> _logger;

        public GetLatestExchangeRateQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetLatestExchangeRateQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ExchangeRateDto>> Handle(GetLatestExchangeRateQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var exchangeRates = await _unitOfWork.CurrencyExchangeRates.GetAllAsync();
                var latestRate = exchangeRates
                    .Where(er => er.FromCurrencyId == request.FromCurrencyId
                                 && er.ToCurrencyId == request.ToCurrencyId
                                 && er.IsActive)
                    .OrderByDescending(er => er.EffectiveDate)
                    .FirstOrDefault();

                if (latestRate == null)
                {
                    return Result<ExchangeRateDto>.NotFound($"No exchange rate found for currencies {request.FromCurrencyId} to {request.ToCurrencyId}");
                }

                var rateDto = _mapper.Map<ExchangeRateDto>(latestRate);
                return Result<ExchangeRateDto>.Success(rateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest exchange rate from {From} to {To}", request.FromCurrencyId, request.ToCurrencyId);
                return Result<ExchangeRateDto>.InternalError("Error retrieving latest exchange rate", new List<string> { ex.Message });
            }
        }
    }
}
