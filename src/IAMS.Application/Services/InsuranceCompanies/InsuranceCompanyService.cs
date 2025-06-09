using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.InsuranceCompanies
{
    public class InsuranceCompanyService : IInsuranceCompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InsuranceCompanyService> _logger;

        public InsuranceCompanyService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InsuranceCompanyService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<InsuranceCompanyDto>> GetAllAsync()
        {
            var companies = await _unitOfWork.InsuranceCompanies.GetAllAsync();
            return _mapper.Map<List<InsuranceCompanyDto>>(companies);
        }

        public async Task<InsuranceCompanyDto?> GetByIdAsync(int id)
        {
            var company = await _unitOfWork.InsuranceCompanies.GetByIdAsync(id);
            return company != null ? _mapper.Map<InsuranceCompanyDto>(company) : null;
        }

        public async Task<InsuranceCompanyDto> CreateAsync(CreateInsuranceCompanyDto companyDto)
        {
            try
            {
                var company = _mapper.Map<InsuranceCompany>(companyDto);
                company.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.InsuranceCompanies.AddAsync(company);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Insurance company created successfully with ID: {CompanyId}", company.Id);
                return _mapper.Map<InsuranceCompanyDto>(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance company");
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateInsuranceCompanyDto companyDto)
        {
            try
            {
                var company = await _unitOfWork.InsuranceCompanies.GetByIdAsync(id);
                if (company == null)
                {
                    throw new InvalidOperationException("Insurance company not found.");
                }

                _mapper.Map(companyDto, company);
                company.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.InsuranceCompanies.Update(company);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Insurance company updated successfully with ID: {CompanyId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance company with ID: {CompanyId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var company = await _unitOfWork.InsuranceCompanies.GetByIdAsync(id);
                if (company == null)
                {
                    throw new InvalidOperationException("Insurance company not found.");
                }

                // Check if company has policies
                var hasPolicies = await _unitOfWork.Policies.ExistsAsync(p => p.InsuranceCompanyId == id);
                if (hasPolicies)
                {
                    throw new InvalidOperationException("Cannot delete insurance company with existing policies.");
                }

                _unitOfWork.InsuranceCompanies.Remove(company);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Insurance company deleted successfully with ID: {CompanyId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting insurance company with ID: {CompanyId}", id);
                throw;
            }
        }

        public async Task<List<InsuranceCompanyDto>> GetActiveCompaniesAsync()
        {
            var companies = await _unitOfWork.InsuranceCompanies.FindAsync(c => c.IsActive);
            return _mapper.Map<List<InsuranceCompanyDto>>(companies);
        }

        public async Task<PagedResult<InsuranceCompanyDto>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            var allCompanies = await _unitOfWork.InsuranceCompanies.GetAllAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                allCompanies = allCompanies.Where(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            var totalCount = allCompanies.Count();
            var items = allCompanies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<InsuranceCompanyDto>
            {
                Items = _mapper.Map<List<InsuranceCompanyDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}