using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Commands.SetExchangeRate
{
    public class SetExchangeRateCommandHandler : IRequestHandler<SetExchangeRateCommand, Result<ExchangeRateDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SetExchangeRateCommandHandler> _logger;

        public SetExchangeRateCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SetExchangeRateCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ExchangeRateDto>> Handle(SetExchangeRateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify currencies exist
                var fromCurrency = await _unitOfWork.Currencies.GetByIdAsync(request.ExchangeRateDto.FromCurrencyId);
                if (fromCurrency == null)
                {
                    return Result<ExchangeRateDto>.NotFound($"From currency with ID {request.ExchangeRateDto.FromCurrencyId} not found");
                }

                var toCurrency = await _unitOfWork.Currencies.GetByIdAsync(request.ExchangeRateDto.ToCurrencyId);
                if (toCurrency == null)
                {
                    return Result<ExchangeRateDto>.NotFound($"To currency with ID {request.ExchangeRateDto.ToCurrencyId} not found");
                }

                // Create exchange rate
                var exchangeRate = _mapper.Map<ExchangeRate>(request.ExchangeRateDto);
                exchangeRate.IsActive = true;
                exchangeRate.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.ExchangeRates.AddAsync(exchangeRate);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Exchange rate created: {From} to {To} = {Rate}",
                    fromCurrency.Code, toCurrency.Code, exchangeRate.Rate);

                var rateDto = _mapper.Map<ExchangeRateDto>(exchangeRate);
                rateDto.FromCurrencyCode = fromCurrency.Code;
                rateDto.FromCurrencySymbol = fromCurrency.Symbol;
                rateDto.ToCurrencyCode = toCurrency.Code;
                rateDto.ToCurrencySymbol = toCurrency.Symbol;

                return Result<ExchangeRateDto>.Success(rateDto, "Exchange rate set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting exchange rate");
                return Result<ExchangeRateDto>.InternalError("Error setting exchange rate", new List<string> { ex.Message });
            }
        }
    }
}
