using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using IAMS.Application.Services.Calculations;
using IAMS.Application.Services.Currencies;
using IAMS.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.UpdatePolicy
{
    public class UpdatePolicyCommandHandler : IRequestHandler<UpdatePolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdatePolicyDto> _validator;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IPolicyCalculatorFactory _calculatorFactory;
        private readonly ICommissionCalculator _commissionCalculator;
        private readonly ICurrencyService _currencyService;
        private readonly IPolicyQueryService _policyQueryService;
        private readonly ILogger<UpdatePolicyCommandHandler> _logger;

        public UpdatePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdatePolicyDto> validator,
            ICurrentTenantService currentTenantService,
            IPolicyCalculatorFactory calculatorFactory,
            ICommissionCalculator commissionCalculator,
            ICurrencyService currencyService,
            IPolicyQueryService policyQueryService,
            ILogger<UpdatePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _currentTenantService = currentTenantService;
            _calculatorFactory = calculatorFactory;
            _commissionCalculator = commissionCalculator;
            _currencyService = currencyService;
            _policyQueryService = policyQueryService;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the request
                var validationResult = await _validator.ValidateAsync(request.PolicyDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<PolicyDto>.ValidationFailure("Doğrulama hatası", errors);
                }

                var existingPolicy = await _unitOfWork.Policies.GetByIdAsync(request.Id);
                if (existingPolicy == null)
                {
                    return Result<PolicyDto>.NotFound("Poliçe bulunamadı");
                }

                // Check business rules
                var businessRuleErrors = await ValidateBusinessRulesAsync(request, existingPolicy);
                if (businessRuleErrors.Any())
                {
                    return Result<PolicyDto>.ValidationFailure("İş kuralları ihlali", businessRuleErrors);
                }

                // Store original values to detect changes
                var originalPolicyTypeId = existingPolicy.PolicyTypeId;
                var originalInsuranceCompanyId = existingPolicy.InsuranceCompanyId;
                var originalPremium = existingPolicy.PremiumAmount;
                var originalCommissionRate = existingPolicy.CommissionRate;

                // Map the updated values
                _mapper.Map(request.PolicyDto, existingPolicy);

                // Look up currency by code and set CurrencyId
                var currency = await _unitOfWork.Currencies.GetByCodeAsync(request.PolicyDto.Currency);
                if (currency == null)
                {
                    return Result<PolicyDto>.ValidationFailure(
                        "Geçersiz para birimi",
                        new List<string> { $"Para birimi '{request.PolicyDto.Currency}' bulunamadı" });
                }
                existingPolicy.CurrencyId = currency.Id;

                // Check if user explicitly changed the commission rate
                bool commissionRateChanged = Math.Abs(originalCommissionRate - existingPolicy.CommissionRate) > 0.01m;

                // Check if factors affecting commission calculation changed
                bool shouldRecalculate =
                    originalPolicyTypeId != existingPolicy.PolicyTypeId ||
                    originalInsuranceCompanyId != existingPolicy.InsuranceCompanyId ||
                    Math.Abs(originalPremium - existingPolicy.PremiumAmount) > 0.01m;

                if (commissionRateChanged)
                {
                    // User explicitly changed the commission rate - use it and recalculate amount only
                    existingPolicy.CommissionAmount = existingPolicy.PremiumAmount * (existingPolicy.CommissionRate / 100);

                    _logger.LogInformation(
                        "Using user-provided commission rate for policy {PolicyNumber}: Rate={Rate}%, Amount={Amount}",
                        existingPolicy.PolicyNumber, existingPolicy.CommissionRate, existingPolicy.CommissionAmount);
                }
                else if (shouldRecalculate)
                {
                    _logger.LogInformation(
                        "Policy {PolicyNumber} requires recalculation due to changes",
                        existingPolicy.PolicyNumber);

                    // Recalculate premium if needed
                    if (existingPolicy.PremiumAmount <= 0 || originalPolicyTypeId != existingPolicy.PolicyTypeId)
                    {
                        var premiumCalculator = await _calculatorFactory.GetCalculatorForPolicyAsync(existingPolicy);
                        existingPolicy.PremiumAmount = await premiumCalculator.CalculatePremiumAsync(existingPolicy);
                    }

                    // Recalculate commission from database since user didn't change the rate
                    var (commissionAmount, commissionRate) = await _commissionCalculator.CalculateCommissionAsync(
                        existingPolicy.PolicyTypeId,
                        existingPolicy.InsuranceCompanyId,
                        existingPolicy.PremiumAmount);

                    existingPolicy.CommissionAmount = commissionAmount;
                    existingPolicy.CommissionRate = commissionRate;

                    _logger.LogInformation(
                        "Recalculated commission from database for policy {PolicyNumber}: Premium={Premium}, Rate={Rate}%, Amount={Amount}",
                        existingPolicy.PolicyNumber, existingPolicy.PremiumAmount, commissionRate, commissionAmount);
                }

                // Recalculate exchange rate to base currency (TRY) and premium in base currency
                var baseCurrencyResult = await _currencyService.GetBaseCurrencyAsync();
                if (baseCurrencyResult.IsSuccess && baseCurrencyResult.Data != null)
                {
                    var baseCurrencyId = baseCurrencyResult.Data.Id;

                    if (currency.Id == baseCurrencyId)
                    {
                        // Policy currency is already the base currency
                        existingPolicy.ExchangeRateToBase = 1;
                        existingPolicy.PremiumAmountInBaseCurrency = existingPolicy.PremiumAmount;
                    }
                    else
                    {
                        // Get exchange rate to base currency
                        var exchangeRateResult = await _currencyService.GetExchangeRateAsync(currency.Id, baseCurrencyId);
                        if (exchangeRateResult.IsSuccess)
                        {
                            existingPolicy.ExchangeRateToBase = exchangeRateResult.Data;
                            existingPolicy.PremiumAmountInBaseCurrency = existingPolicy.PremiumAmount * exchangeRateResult.Data;

                            _logger.LogInformation(
                                "Updated exchange rate for policy {PolicyNumber}: {FromCurrency} to {ToCurrency} = {Rate}, Premium in base currency: {PremiumInBase}",
                                existingPolicy.PolicyNumber, currency.Code, baseCurrencyResult.Data.Code,
                                exchangeRateResult.Data, existingPolicy.PremiumAmountInBaseCurrency);
                        }
                        else
                        {
                            // Fallback to 1:1 if exchange rate not found
                            existingPolicy.ExchangeRateToBase = 1;
                            existingPolicy.PremiumAmountInBaseCurrency = existingPolicy.PremiumAmount;
                            _logger.LogWarning(
                                "Could not get exchange rate for currency {CurrencyCode} to base currency, defaulting to 1:1. Message: {Message}",
                                currency.Code, exchangeRateResult.Message);
                        }
                    }
                }
                else
                {
                    // Could not get base currency, default to 1:1
                    existingPolicy.ExchangeRateToBase = 1;
                    existingPolicy.PremiumAmountInBaseCurrency = existingPolicy.PremiumAmount;
                    _logger.LogWarning("Could not get base currency, defaulting to 1:1 exchange rate");
                }

                existingPolicy.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Policies.Update(existingPolicy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var updatedPolicyDto = _mapper.Map<PolicyDto>(existingPolicy);
                _logger.LogInformation("Policy updated successfully with ID: {PolicyId}", request.Id);

                return Result<PolicyDto>.Success(updatedPolicyDto, "Poliçe başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.InternalError("Poliçe güncellenirken beklenmeyen bir hata oluştu");
            }
        }

        private async Task<List<string>> ValidateBusinessRulesAsync(UpdatePolicyCommand request, Domain.Entities.Policy existingPolicy)
        {
            var errors = new List<string>();

            try
            {
                // Business rule: Cannot change policy number if it's already active
                if (existingPolicy.Status == Domain.Enums.PolicyStatus.Active &&
                    existingPolicy.PolicyNumber != request.PolicyDto.PolicyNumber)
                {
                    errors.Add("Aktif poliçenin numarası değiştirilemez");
                }

                // Business rule: Cannot change to expired status if not yet expired
                if (request.PolicyDto.Status == Domain.Enums.PolicyStatus.Expired &&
                    existingPolicy.EndDate > DateTime.Now)
                {
                    errors.Add("Henüz süresi dolmamış poliçe 'Süresi Dolmuş' durumuna getirilemez");
                }

                // Business rule: Cannot reactivate if premium payment is overdue
                if (request.PolicyDto.Status == Domain.Enums.PolicyStatus.Active &&
                    existingPolicy.Status != Domain.Enums.PolicyStatus.Active)
                {
                    // Check if there are overdue payments
                    var hasOverduePayments = await _policyQueryService.HasOverduePaymentsAsync(existingPolicy.Id);
                    if (hasOverduePayments)
                    {
                        errors.Add("Gecikmiş ödeme bulunan poliçe aktif duruma getirilemez");
                    }
                }

                // Add more business rules as needed...
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during business rule validation for policy {PolicyId}", request.Id);
                errors.Add("İş kuralları doğrulanırken bir hata oluştu");
            }

            return errors;
        }
    }
}