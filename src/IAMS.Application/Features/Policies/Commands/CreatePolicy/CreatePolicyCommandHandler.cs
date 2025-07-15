using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
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
        private readonly ILogger<CreatePolicyCommandHandler> _logger;

        public CreatePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreatePolicyDto> validator,
            IPolicyNumberGenerator policyNumberGenerator,
            ILogger<CreatePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _policyNumberGenerator = policyNumberGenerator;
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

                // Generate policy number if not provided
                if (string.IsNullOrEmpty(policy.PolicyNumber))
                {
                    policy.PolicyNumber = await _policyNumberGenerator.GenerateAsync(
                        policy.TenantId, policy.InsuranceCompanyId, policy.PolicyTypeId);
                }

                // Calculate commission
                policy.CalculateCommission();

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