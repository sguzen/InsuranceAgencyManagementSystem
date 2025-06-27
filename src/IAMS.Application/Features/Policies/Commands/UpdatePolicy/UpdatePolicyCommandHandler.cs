using MediatR;
using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.UpdatePolicy
{
    public class UpdatePolicyCommandHandler : IRequestHandler<UpdatePolicyCommand, Result<PolicyDto>>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPolicyValidator _policyValidator;
        private readonly ILogger<UpdatePolicyCommandHandler> _logger;

        public UpdatePolicyCommandHandler(
            IPolicyRepository policyRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPolicyValidator policyValidator,
            ILogger<UpdatePolicyCommandHandler> logger)
        {
            _policyRepository = policyRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _policyValidator = policyValidator;
            _logger = logger;
        }

        public async Task<Result<PolicyDto>> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating policy with ID: {PolicyId}", request.Id);

                // Get existing policy
                var policy = await _policyRepository.GetByIdAsync(request.Id);
                if (policy == null)
                {
                    return Result<PolicyDto>.NotFound("Policy not found");
                }

                // Update policy properties
                policy.UpdateDates(request.PolicyDto.StartDate, request.PolicyDto.EndDate, "System");
                policy.UpdatePremium(request.PolicyDto.PremiumAmount, "System");
                policy.CalculateCommission(request.PolicyDto.CommissionRate, "System");
                policy.UpdateNotes(request.PolicyDto.Notes, "System");

                // Handle status changes
                if (request.PolicyDto.Status != policy.Status)
                {
                    switch (request.PolicyDto.Status)
                    {
                        case Domain.Enums.PolicyStatus.Active:
                            policy.ActivatePolicy("System");
                            break;
                        case Domain.Enums.PolicyStatus.Cancelled:
                            policy.CancelPolicy("System", "Updated via API");
                            break;
                        case Domain.Enums.PolicyStatus.Suspended:
                            policy.SuspendPolicy("System", "Updated via API");
                            break;
                    }
                }

                // Validate updated policy
                var validationErrors = await _policyValidator.ValidatePolicyAsync(policy);
                if (validationErrors.Any())
                {
                    return Result<PolicyDto>.Failure("Validation failed", validationErrors);
                }

                // Save changes
                _policyRepository.Update(policy);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var policyDto = _mapper.Map<PolicyDto>(policy);

                _logger.LogInformation("Policy updated successfully: {PolicyId}", policy.Id);

                return Result<PolicyDto>.Success(policyDto, "Policy updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy with ID: {PolicyId}", request.Id);
                return Result<PolicyDto>.InternalError("An error occurred while updating the policy");
            }
        }
    }
}