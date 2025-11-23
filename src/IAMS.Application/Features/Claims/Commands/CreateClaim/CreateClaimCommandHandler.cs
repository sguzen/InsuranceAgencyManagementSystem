using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Commands.CreateClaim
{
    public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Result<PolicyClaimDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimCalculator _claimCalculator;
        private readonly ILogger<CreateClaimCommandHandler> _logger;

        public CreateClaimCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IClaimCalculator claimCalculator,
            ILogger<CreateClaimCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimCalculator = claimCalculator;
            _logger = logger;
        }

        public async Task<Result<PolicyClaimDto>> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify policy exists
                var policy = await _unitOfWork.Policies.GetByIdAsync(request.ClaimDto.PolicyId);
                if (policy == null)
                {
                    return Result<PolicyClaimDto>.NotFound($"Policy with ID {request.ClaimDto.PolicyId} not found");
                }

                var claim = _mapper.Map<PolicyClaim>(request.ClaimDto);

                // Validate policy eligibility for claims
                if (!_claimCalculator.IsPolicyEligibleForClaim(policy, claim.ClaimDate))
                {
                    return Result<PolicyClaimDto>.ValidationFailure(
                        "Policy is not eligible for claims",
                        new List<string>
                        {
                            "The policy is not active or the claim date is outside the policy coverage period"
                        });
                }

                // Validate claim amount against policy coverage
                if (!_claimCalculator.ValidateClaimAmount(policy, claim.ClaimAmount))
                {
                    var maxClaimable = _claimCalculator.GetMaximumClaimableAmount(policy);
                    return Result<PolicyClaimDto>.ValidationFailure(
                        "Claim amount exceeds policy coverage",
                        new List<string>
                        {
                            $"The claimed amount exceeds the maximum coverage of {maxClaimable:C}. " +
                            $"Deductible amount: {policy.DeductibleAmount ?? 0:C}"
                        });
                }

                // Calculate actual payable amount (after deductibles)
                var payableAmount = _claimCalculator.CalculatePayableAmount(policy, claim.ClaimAmount);
                claim.SettledAmount = payableAmount;

                _logger.LogInformation(
                    "Claim for policy {PolicyNumber}: Claimed={Claimed}, Deductible={Deductible}, Payable={Payable}",
                    policy.PolicyNumber, claim.ClaimAmount, policy.DeductibleAmount ?? 0, payableAmount);

                claim.CreatedOn = DateTime.UtcNow;

                // Generate claim number if not provided
                if (string.IsNullOrEmpty(claim.ClaimNumber))
                {
                    claim.ClaimNumber = await GenerateClaimNumberAsync();
                }

                await _unitOfWork.PolicyClaims.AddAsync(claim);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Claim created successfully with ID: {ClaimId}", claim.Id);

                var claimDto = _mapper.Map<PolicyClaimDto>(claim);
                return Result<PolicyClaimDto>.Success(claimDto, "Claim created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim for policy ID: {PolicyId}", request.ClaimDto.PolicyId);
                return Result<PolicyClaimDto>.InternalError("Error creating claim", new List<string> { ex.Message });
            }
        }

        private async Task<string> GenerateClaimNumberAsync()
        {
            // Generate claim number in format: CLM-YYYYMMDD-XXXX
            var date = DateTime.UtcNow;
            var prefix = $"CLM-{date:yyyyMMdd}";

            var allClaims = await _unitOfWork.PolicyClaims.GetAllAsync();
            var todayClaims = allClaims
                .Where(c => c.ClaimNumber.StartsWith(prefix))
                .ToList();

            var sequence = todayClaims.Count + 1;
            return $"{prefix}-{sequence:D4}";
        }
    }
}
