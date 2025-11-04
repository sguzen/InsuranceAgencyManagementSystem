using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Commands.CreateClaim
{
    public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Result<PolicyClaimDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateClaimCommandHandler> _logger;

        public CreateClaimCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateClaimCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
