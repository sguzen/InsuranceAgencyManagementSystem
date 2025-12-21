using AutoMapper;
using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetActiveCurrencies
{
    public class GetActiveCurrenciesQueryHandler : IRequestHandler<GetActiveCurrenciesQuery, Result<List<CurrencyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActiveCurrenciesQueryHandler> _logger;

        public GetActiveCurrenciesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetActiveCurrenciesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CurrencyDto>>> Handle(GetActiveCurrenciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var activeCurrencies = currencies.Where(c => c.IsActive).ToList();
                var currencyDtos = _mapper.Map<List<CurrencyDto>>(activeCurrencies);

                _logger.LogInformation("Retrieved {Count} active currencies", currencyDtos.Count);
                return Result<List<CurrencyDto>>.Success(currencyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active currencies");
                return Result<List<CurrencyDto>>.InternalError("Error retrieving active currencies", new List<string> { ex.Message });
            }
        }
    }
}
