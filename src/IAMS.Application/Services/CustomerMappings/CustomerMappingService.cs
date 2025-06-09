using AutoMapper;
using IAMS.Application.DTOs.CustomerMapping;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.CustomerMappings
{
    public class CustomerMappingService : ICustomerMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerMappingService> _logger;

        public CustomerMappingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerMappingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CustomerMappingDto>> GetMappingsByCustomerIdAsync(int customerId)
        {
            var mappings = await _unitOfWork.CustomerInsuranceCompanies.GetByCustomerIdAsync(customerId);
            return _mapper.Map<List<CustomerMappingDto>>(mappings);
        }

        public async Task<List<CustomerMappingDto>> GetMappingsByCompanyIdAsync(int companyId)
        {
            var mappings = await _unitOfWork.CustomerInsuranceCompanies.GetByInsuranceCompanyIdAsync(companyId);
            return _mapper.Map<List<CustomerMappingDto>>(mappings);
        }

        public async Task<CustomerMappingDto> CreateMappingAsync(CreateCustomerMappingDto mappingDto)
        {
            try
            {
                // Check if mapping already exists
                var existingMapping = await _unitOfWork.CustomerInsuranceCompanies
                    .GetMappingAsync(mappingDto.CustomerId, mappingDto.InsuranceCompanyId);

                if (existingMapping != null)
                {
                    throw new InvalidOperationException("Mapping already exists for this customer and insurance company.");
                }

                var mapping = _mapper.Map<CustomerInsuranceCompany>(mappingDto);
                mapping.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.CustomerInsuranceCompanies.AddAsync(mapping);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer mapping created successfully with ID: {MappingId}", mapping.Id);
                return _mapper.Map<CustomerMappingDto>(mapping);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer mapping");
                throw;
            }
        }

        public async Task UpdateMappingAsync(int id, UpdateCustomerMappingDto mappingDto)
        {
            try
            {
                var mapping = await _unitOfWork.CustomerInsuranceCompanies.GetByIdAsync(id);
                if (mapping == null)
                {
                    throw new InvalidOperationException("Customer mapping not found.");
                }

                _mapper.Map(mappingDto, mapping);
                mapping.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.CustomerInsuranceCompanies.Update(mapping);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer mapping updated successfully with ID: {MappingId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer mapping with ID: {MappingId}", id);
                throw;
            }
        }

        public async Task DeleteMappingAsync(int id)
        {
            try
            {
                var mapping = await _unitOfWork.CustomerInsuranceCompanies.GetByIdAsync(id);
                if (mapping == null)
                {
                    throw new InvalidOperationException("Customer mapping not found.");
                }

                _unitOfWork.CustomerInsuranceCompanies.Remove(mapping);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer mapping deleted successfully with ID: {MappingId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer mapping with ID: {MappingId}", id);
                throw;
            }
        }

        public async Task<string?> GetExternalCustomerIdAsync(int customerId, int companyId)
        {
            return await _unitOfWork.CustomerInsuranceCompanies.GetExternalCustomerIdAsync(customerId, companyId);
        }
    }
}