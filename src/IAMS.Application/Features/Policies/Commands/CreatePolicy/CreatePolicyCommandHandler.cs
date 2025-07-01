using AutoMapper;
using FluentValidation;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.CreatePolicy
{
    public class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, Result<PolicyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreatePolicyDto> _validator;
        private readonly ILogger<CreatePolicyCommandHandler> _logger;

        public CreatePolicyCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreatePolicyDto> validator,
            ILogger<CreatePolicyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the request
                var validationResult = await _validator.ValidateAsync(request.PolicyDto, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<PolicyDto>.Failure("Validation failed", errors);
                }

                // Check if customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.PolicyDto.CustomerId);
                if (customer == null)
                {
                    return Result<PolicyDto>.Failure("Customer not found");
                }

                // Check if insurance company exists
                var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(request.PolicyDto.InsuranceCompanyId);
                if (insuranceCompany == null)
                {
                    return Result<PolicyDto>.Failure("Insurance company not found");
                }

                // Check if policy number already exists
                var existingPolicy = await _unitOfWork.Policies.GetByPolicyNumberAsync(request.PolicyDto.PolicyNumber);
                if (existingPolicy != null)
                {
                    return Result<PolicyDto>.Failure("A policy with this number already exists");
                }

                // Create the policy
                var policy = _mapper.Map<Policy>(request.PolicyDto);
                policy.CreatedOn = DateTime.UtcNow;
                policy.Status = Domain.Enums.PolicyStatus.Draft;

                // Calculate commission amount
                policy.CommissionAmount = policy.PremiumAmount * (policy.CommissionRate / 100);

                await _unitOfWork.Policies.AddAsync(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var policyDto = _mapper.Map<PolicyDto>(policy);
                _logger.LogInformation("Policy created successfully with ID: {PolicyId}", policy.Id);

                return Result<PolicyDto>.Success(policyDto, "Policy created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy");
                return Result<PolicyDto>.Failure("An error occurred while creating the policy");
            }
        }
    }
}