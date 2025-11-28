using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Application.Services.Currencies;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.PopulatePolicyExchangeRates
{
    public class PopulatePolicyExchangeRatesCommandHandler
        : IRequestHandler<PopulatePolicyExchangeRatesCommand, Result<PopulatePolicyExchangeRatesResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<PopulatePolicyExchangeRatesCommandHandler> _logger;

        public PopulatePolicyExchangeRatesCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrencyService currencyService,
            ILogger<PopulatePolicyExchangeRatesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currencyService = currencyService;
            _logger = logger;
        }

        public async Task<Result<PopulatePolicyExchangeRatesResult>> Handle(
            PopulatePolicyExchangeRatesCommand request,
            CancellationToken cancellationToken)
        {
            var result = new PopulatePolicyExchangeRatesResult();

            try
            {
                _logger.LogInformation("Starting exchange rate population. OnlyMissing: {OnlyMissing}, UseHistoricalRates: {UseHistoricalRates}",
                    request.OnlyMissing, request.UseHistoricalRates);

                // Get base currency
                var baseCurrencyResult = await _currencyService.GetBaseCurrencyAsync();
                if (!baseCurrencyResult.IsSuccess || baseCurrencyResult.Data == null)
                {
                    return Result<PopulatePolicyExchangeRatesResult>.Failure(
                        "Could not retrieve base currency",
                        new List<string> { "Base currency not configured in the system" });
                }

                var baseCurrencyId = baseCurrencyResult.Data.Id;
                _logger.LogInformation("Using base currency: {CurrencyCode} (ID: {CurrencyId})",
                    baseCurrencyResult.Data.Code, baseCurrencyId);

                // Get all policies
                var policies = await _unitOfWork.Policies.GetAllAsync();
                var policiesList = policies.ToList();
                result.TotalPolicies = policiesList.Count;

                _logger.LogInformation("Found {Count} total policies", result.TotalPolicies);

                foreach (var policy in policiesList)
                {
                    try
                    {
                        // Skip if only processing missing and this policy already has exchange rate
                        if (request.OnlyMissing && policy.ExchangeRateToBase.HasValue && policy.ExchangeRateToBase.Value > 0)
                        {
                            result.SkippedPolicies++;
                            continue;
                        }

                        // Get the currency for this policy
                        var currency = await _unitOfWork.Currencies.GetByIdAsync(policy.CurrencyId);
                        if (currency == null)
                        {
                            _logger.LogWarning("Policy {PolicyNumber} has invalid currency ID {CurrencyId}",
                                policy.PolicyNumber, policy.CurrencyId);
                            result.FailedPolicies++;
                            result.Errors.Add($"Policy {policy.PolicyNumber}: Invalid currency ID {policy.CurrencyId}");
                            continue;
                        }

                        // If policy currency is the base currency, set 1:1 rate
                        if (currency.Id == baseCurrencyId)
                        {
                            policy.ExchangeRateToBase = 1;
                            policy.PremiumAmountInBaseCurrency = policy.PremiumAmount;
                            result.ProcessedPolicies++;
                            _logger.LogDebug("Policy {PolicyNumber}: Base currency, set 1:1 rate", policy.PolicyNumber);
                            continue;
                        }

                        // Get exchange rate
                        DateTime? rateDate = request.UseHistoricalRates ? policy.CreatedOn : null;
                        var exchangeRateResult = await _currencyService.GetExchangeRateAsync(
                            currency.Id,
                            baseCurrencyId,
                            rateDate);

                        if (exchangeRateResult.IsSuccess && exchangeRateResult.Data > 0)
                        {
                            policy.ExchangeRateToBase = exchangeRateResult.Data;
                            policy.PremiumAmountInBaseCurrency = policy.PremiumAmount * exchangeRateResult.Data;
                            result.ProcessedPolicies++;

                            _logger.LogDebug(
                                "Policy {PolicyNumber}: Set exchange rate {Rate} for {Currency} (Premium: {Premium} -> {PremiumInBase})",
                                policy.PolicyNumber, exchangeRateResult.Data, currency.Code,
                                policy.PremiumAmount, policy.PremiumAmountInBaseCurrency);
                        }
                        else
                        {
                            // Fallback to 1:1 if no exchange rate found
                            policy.ExchangeRateToBase = 1;
                            policy.PremiumAmountInBaseCurrency = policy.PremiumAmount;
                            result.ProcessedPolicies++;

                            var warningMsg = $"Policy {policy.PolicyNumber}: No exchange rate found for {currency.Code}, defaulting to 1:1";
                            _logger.LogWarning(warningMsg);
                            result.Errors.Add(warningMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedPolicies++;
                        var errorMsg = $"Policy {policy.PolicyNumber}: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        _logger.LogError(ex, "Error processing policy {PolicyNumber}", policy.PolicyNumber);
                    }
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Exchange rate population completed. Total: {Total}, Processed: {Processed}, Skipped: {Skipped}, Failed: {Failed}",
                    result.TotalPolicies, result.ProcessedPolicies, result.SkippedPolicies, result.FailedPolicies);

                return Result<PopulatePolicyExchangeRatesResult>.Success(
                    result,
                    $"Processed {result.ProcessedPolicies} policies successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during exchange rate population");
                return Result<PopulatePolicyExchangeRatesResult>.InternalError(
                    "An error occurred while populating exchange rates");
            }
        }
    }
}
