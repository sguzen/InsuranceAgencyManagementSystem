using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using IAMS.Domain.Exceptions;
using IAMS.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.CreatePolicy
{
    public class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreatePolicyDto> _validator;
        private readonly IPolicyNumberGenerator _policyNumberGenerator;
        private readonly IPolicyCalculatorFactory _calculatorFactory;
        private readonly ICommissionCalculator _commissionCalculator;
        private readonly ILogger<CreatePolicyCommandHandler> _logger;

        public CreatePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreatePolicyDto> validator,
            IPolicyNumberGenerator policyNumberGenerator,
            IPolicyCalculatorFactory calculatorFactory,
            ICommissionCalculator commissionCalculator,
            ILogger<CreatePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _policyNumberGenerator = policyNumberGenerator;
            _calculatorFactory = calculatorFactory;
            _commissionCalculator = commissionCalculator;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the command
                var validationResult = await _validator.ValidateAsync(request.PolicyDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<PolicyDto>.ValidationFailure("Doğrulama hatası", errors);
                }

                var policy = _mapper.Map<Domain.Entities.Policy>(request.PolicyDto);

                // Look up currency by code and set CurrencyId
                var currency = await _unitOfWork.Currencies.GetByCodeAsync(request.PolicyDto.Currency);
                if (currency == null)
                {
                    return Result<PolicyDto>.ValidationFailure(
                        "Geçersiz para birimi",
                        new List<string> { $"Para birimi '{request.PolicyDto.Currency}' bulunamadı" });
                }
                policy.CurrencyId = currency.Id;

                // Generate policy number if not provided
                if (string.IsNullOrEmpty(policy.PolicyNumber))
                {
                    policy.PolicyNumber = await _policyNumberGenerator.GenerateAsync(policy.InsuranceCompanyId, policy.PolicyTypeId);
                }

                // Calculate premium using type-specific calculator (if not already set)
                if (policy.PremiumAmount <= 0)
                {
                    var premiumCalculator = await _calculatorFactory.GetCalculatorForPolicyAsync(policy);
                    policy.PremiumAmount = await premiumCalculator.CalculatePremiumAsync(policy);
                    _logger.LogInformation(
                        "Calculated premium for policy {PolicyNumber}: {Premium}",
                        policy.PolicyNumber, policy.PremiumAmount);
                }

                // Calculate commission using database lookup
                var (commissionAmount, commissionRate) = await _commissionCalculator.CalculateCommissionAsync(
                    policy.PolicyTypeId,
                    policy.InsuranceCompanyId,
                    policy.PremiumAmount);

                policy.CommissionAmount = commissionAmount;
                policy.CommissionRate = commissionRate;

                _logger.LogInformation(
                    "Calculated commission for policy {PolicyNumber}: Rate={Rate}%, Amount={Amount}",
                    policy.PolicyNumber, commissionRate, commissionAmount);

                // Validate business rules
                policy.Validate();

                policy.CreatedOn = DateTime.UtcNow;
                await _unitOfWork.Policies.AddAsync(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var policyDto = _mapper.Map<PolicyDto>(policy);
                _logger.LogInformation("Policy created successfully with ID: {PolicyId}", policy.Id);

                return Result<PolicyDto>.Success(policyDto, "Poliçe başarıyla oluşturuldu");
            }
            catch (PolicyValidationException ex)
            {
                return Result<PolicyDto>.ValidationFailure("Poliçe doğrulama hatası", ex.ValidationErrors.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                return Result<PolicyDto>.InternalError("Poliçe oluşturulurken beklenmeyen bir hata oluştu");
            }
        }
    }
}