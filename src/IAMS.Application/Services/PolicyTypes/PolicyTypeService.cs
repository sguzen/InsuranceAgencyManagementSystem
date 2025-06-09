using AutoMapper;
using IAMS.Application.DTOs.PolicyType;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.PolicyTypes
{
    public class PolicyTypeService : IPolicyTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyTypeService> _logger;

        public PolicyTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PolicyTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyTypeDto>> GetAllAsync()
        {
            var policyTypes = await _unitOfWork.PolicyTypes.GetAllAsync();
            return _mapper.Map<List<PolicyTypeDto>>(policyTypes);
        }

        public async Task<PolicyTypeDto?> GetByIdAsync(int id)
        {
            var policyType = await _unitOfWork.PolicyTypes.GetByIdAsync(id);
            return policyType != null ? _mapper.Map<PolicyTypeDto>(policyType) : null;
        }

        public async Task<PolicyTypeDto> CreateAsync(CreatePolicyTypeDto policyTypeDto)
        {
            try
            {
                var policyType = _mapper.Map<PolicyType>(policyTypeDto);
                policyType.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.PolicyTypes.AddAsync(policyType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy type created successfully with ID: {PolicyTypeId}", policyType.Id);
                return _mapper.Map<PolicyTypeDto>(policyType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating policy type");
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdatePolicyTypeDto policyTypeDto)
        {
            try
            {
                var policyType = await _unitOfWork.PolicyTypes.GetByIdAsync(id);
                if (policyType == null)
                {
                    throw new InvalidOperationException("Policy type not found.");
                }

                _mapper.Map(policyTypeDto, policyType);
                policyType.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyTypes.Update(policyType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy type updated successfully with ID: {PolicyTypeId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy type with ID: {PolicyTypeId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var policyType = await _unitOfWork.PolicyTypes.GetByIdAsync(id);
                if (policyType == null)
                {
                    throw new InvalidOperationException("Policy type not found.");
                }

                // Check if policy type has policies
                var hasPolicies = await _unitOfWork.Policies.ExistsAsync(p => p.PolicyTypeId == id);
                if (hasPolicies)
                {
                    throw new InvalidOperationException("Cannot delete policy type with existing policies.");
                }

                _unitOfWork.PolicyTypes.Remove(policyType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Policy type deleted successfully with ID: {PolicyTypeId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy type with ID: {PolicyTypeId}", id);
                throw;
            }
        }

        public async Task<List<PolicyTypeDto>> GetActiveTypesAsync()
        {
            var policyTypes = await _unitOfWork.PolicyTypes.FindAsync(pt => pt.IsActive);
            return _mapper.Map<List<PolicyTypeDto>>(policyTypes);
        }
    }
}