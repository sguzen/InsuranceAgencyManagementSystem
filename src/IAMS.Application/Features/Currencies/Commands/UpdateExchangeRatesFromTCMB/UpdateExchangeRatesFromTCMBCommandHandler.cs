using IAMS.Application.DTOs.Currency;
using IAMS.Application.Features.Currencies.Commands.SetExchangeRate;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;

namespace IAMS.Application.Features.Currencies.Commands.UpdateExchangeRatesFromTCMB
{
    public class UpdateExchangeRatesFromTCMBCommandHandler : IRequestHandler<UpdateExchangeRatesFromTCMBCommand, Result>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateExchangeRatesFromTCMBCommandHandler> _logger;
        private const string TCMB_URL = "https://www.tcmb.gov.tr/kurlar/today.xml";

        public UpdateExchangeRatesFromTCMBCommandHandler(
            IHttpClientFactory httpClientFactory,
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<UpdateExchangeRatesFromTCMBCommandHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateExchangeRatesFromTCMBCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Get base currency (should be TRY for TCMB)
                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var baseCurrency = currencies.FirstOrDefault(c => c.IsBaseCurrency);
                if (baseCurrency == null)
                {
                    return Result.NotFound("No base currency configured");
                }

                // Fetch XML from TCMB
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync(TCMB_URL, cancellationToken);

                // Parse XML
                var xmlDoc = XDocument.Parse(response);
                var currencyNodes = xmlDoc.Descendants("Currency");

                var ratesAdded = 0;
                var errors = new List<string>();

                foreach (var currencyNode in currencyNodes)
                {
                    try
                    {
                        var currencyCode = currencyNode.Attribute("CurrencyCode")?.Value;
                        if (string.IsNullOrEmpty(currencyCode))
                            continue;

                        var forexBuyingElement = currencyNode.Element("ForexBuying");
                        if (forexBuyingElement == null || string.IsNullOrEmpty(forexBuyingElement.Value))
                            continue;

                        // Find currency in database
                        var currency = currencies.FirstOrDefault(c =>
                            c.Code.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
                        if (currency == null)
                        {
                            _logger.LogWarning("Currency not found in database: {Code}", currencyCode);
                            continue;
                        }

                        // Parse rate
                        if (!decimal.TryParse(forexBuyingElement.Value,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var rate))
                        {
                            _logger.LogWarning("Could not parse rate for {Code}: {Value}",
                                currencyCode, forexBuyingElement.Value);
                            continue;
                        }

                        // Get unit (some currencies are quoted per 100 units)
                        var unitElement = currencyNode.Element("Unit");
                        if (unitElement != null && decimal.TryParse(unitElement.Value, out var unit) && unit > 1)
                        {
                            rate = rate / unit;
                        }

                        // Create exchange rate DTO
                        var exchangeRateDto = new CreateExchangeRateDto
                        {
                            FromCurrencyId = currency.Id,
                            ToCurrencyId = baseCurrency.Id,
                            Rate = rate,
                            EffectiveDate = DateTime.UtcNow.Date,
                            ExpiryDate = DateTime.UtcNow.Date.AddDays(1),
                            Source = "TCMB"
                        };

                        // Set exchange rate
                        var result = await _mediator.Send(new SetExchangeRateCommand(exchangeRateDto), cancellationToken);
                        if (result.IsSuccess)
                        {
                            ratesAdded++;
                            _logger.LogInformation("Updated exchange rate for {Code}: {Rate}",
                                currencyCode, rate);
                        }
                        else
                        {
                            errors.Add($"Failed to set rate for {currencyCode}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing currency node");
                        errors.Add($"Error processing currency: {ex.Message}");
                    }
                }

                if (ratesAdded == 0)
                {
                    return Result.Failure("No exchange rates were updated from TCMB", errors);
                }

                _logger.LogInformation("Successfully updated {Count} exchange rates from TCMB", ratesAdded);
                return Result.Success($"Successfully updated {ratesAdded} exchange rates from TCMB");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching exchange rates from TCMB");
                return Result.InternalError("Error fetching exchange rates from TCMB", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates from TCMB");
                return Result.InternalError("Error updating exchange rates from TCMB", new List<string> { ex.Message });
            }
        }
    }
}
