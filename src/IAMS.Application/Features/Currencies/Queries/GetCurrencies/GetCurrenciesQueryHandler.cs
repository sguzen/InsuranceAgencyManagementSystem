using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencies
{
    public class GetCurrenciesQueryHandler
        : IRequestHandler<GetCurrenciesQuery, Result<List<CurrencyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCurrenciesQueryHandler> _logger;

        public GetCurrenciesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCurrenciesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<CurrencyDto>>> Handle(
            GetCurrenciesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var currencies = await _unitOfWork.Currencies
                    .AsQueryable()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Code)
                    .ToListAsync(cancellationToken);

                var result = currencies.Select(c => new CurrencyDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Symbol = c.Symbol,
                    IsActive = c.IsActive
                }).ToList();

                return Result<List<CurrencyDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting currencies");
                return Result<List<CurrencyDto>>.InternalError(
                    "Para birimleri getirilirken bir hata oluştu",
                    new List<string> { ex.Message });
            }
        }
    }
}
