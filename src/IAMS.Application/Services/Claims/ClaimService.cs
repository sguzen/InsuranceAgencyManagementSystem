using AutoMapper;
using IAMS.Application.DTOs.Claim;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Claims
{
    public class ClaimService : IClaimService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClaimService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyClaimDto>> GetClaimsByPolicyIdAsync(int policyId)
        {
            try
            {
                var claims = await _unitOfWork.PolicyClaims.GetClaimsByPolicyIdAsync(policyId);
                return _mapper.Map<List<PolicyClaimDto>>(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims for policy ID: {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<PolicyClaimDto?> GetByIdAsync(int id)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(id);
                return claim != null ? _mapper.Map<PolicyClaimDto>(claim) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claim with ID: {ClaimId}", id);
                throw;
            }
        }

        public async Task<PolicyClaimDto> CreateAsync(CreatePolicyClaimDto claimDto)
        {
            try
            {
                // Verify policy exists
                var policy = await _unitOfWork.Policies.GetByIdAsync(claimDto.PolicyId);
                if (policy == null)
                {
                    throw new InvalidOperationException($"Policy with ID {claimDto.PolicyId} not found.");
                }

                var claim = _mapper.Map<PolicyClaim>(claimDto);
                claim.CreatedOn = DateTime.UtcNow;

                // Generate claim number if not provided
                if (string.IsNullOrEmpty(claim.ClaimNumber))
                {
                    claim.ClaimNumber = await GenerateClaimNumberAsync();
                }

                await _unitOfWork.PolicyClaims.AddAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Claim created successfully with ID: {ClaimId}", claim.Id);
                return _mapper.Map<PolicyClaimDto>(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim");
                throw;
            }
        }

        public async Task UpdateClaimStatusAsync(int id, ClaimStatus status)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    throw new InvalidOperationException($"Claim with ID {id} not found.");
                }

                claim.Status = status;
                claim.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyClaims.Update(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Claim status updated successfully for ID: {ClaimId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating claim status for ID: {ClaimId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var claim = await _unitOfWork.PolicyClaims.GetByIdAsync(id);
                if (claim == null)
                {
                    throw new InvalidOperationException($"Claim with ID {id} not found.");
                }

                _unitOfWork.PolicyClaims.Remove(claim);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Claim deleted successfully with ID: {ClaimId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting claim with ID: {ClaimId}", id);
                throw;
            }
        }

        public async Task<List<PolicyClaimDto>> GetClaimsByStatusAsync(ClaimStatus status)
        {
            try
            {
                var claims = await _unitOfWork.PolicyClaims.GetClaimsByStatusAsync(status);
                return _mapper.Map<List<PolicyClaimDto>>(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims by status: {Status}", status);
                throw;
            }
        }

        public async Task<PagedResult<PolicyClaimDto>> GetClaimsPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            try
            {
                var query = (await _unitOfWork.PolicyClaims.GetAllAsync()).AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.ClaimNumber.ToLower().Contains(searchTerm) ||
                        c.Description.ToLower().Contains(searchTerm) ||
                        (c.Notes != null && c.Notes.ToLower().Contains(searchTerm)));
                }

                var totalCount = query.Count();
                var claims = query
                    .OrderByDescending(c => c.CreatedOn)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var claimDtos = _mapper.Map<List<PolicyClaimDto>>(claims);

                return PagedResult<PolicyClaimDto>.Success(claimDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged claims");
                return PagedResult<PolicyClaimDto>.InternalError("Error retrieving claims", new List<string> { ex.Message }, pageNumber, pageSize);
            }
        }

        public async Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId)
        {
            try
            {
                return await _unitOfWork.PolicyClaims.GetTotalClaimAmountByPolicyIdAsync(policyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total claim amount for policy ID: {PolicyId}", policyId);
                throw;
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
