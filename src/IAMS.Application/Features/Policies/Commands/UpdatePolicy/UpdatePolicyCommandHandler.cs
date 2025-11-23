using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Application.Services.Calculations;
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
        private readonly ILogger<UpdatePolicyCommandHandler> _logger;

        public UpdatePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdatePolicyDto> validator,
            ICurrentTenantService currentTenantService,
            IPolicyCalculatorFactory calculatorFactory,
            ICommissionCalculator commissionCalculator,
            ILogger<UpdatePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _currentTenantService = currentTenantService;
            _calculatorFactory = calculatorFactory;
            _commissionCalculator = commissionCalculator;
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

                // Recalculate if policy type or insurance company changed, or if premium changed significantly
                bool shouldRecalculate =
                    originalPolicyTypeId != existingPolicy.PolicyTypeId ||
                    originalInsuranceCompanyId != existingPolicy.InsuranceCompanyId ||
                    Math.Abs(originalPremium - existingPolicy.PremiumAmount) > 0.01m;

                if (shouldRecalculate)
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

                    // Recalculate commission
                    var (commissionAmount, commissionRate) = await _commissionCalculator.CalculateCommissionAsync(
                        existingPolicy.PolicyTypeId,
                        existingPolicy.InsuranceCompanyId,
                        existingPolicy.PremiumAmount);

                    existingPolicy.CommissionAmount = commissionAmount;
                    existingPolicy.CommissionRate = commissionRate;

                    _logger.LogInformation(
                        "Recalculated for policy {PolicyNumber}: Premium={Premium}, Commission={Commission}",
                        existingPolicy.PolicyNumber, existingPolicy.PremiumAmount, commissionAmount);
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
                    var hasOverduePayments = await _unitOfWork.Policies.HasOverduePaymentsAsync(existingPolicy.Id);
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