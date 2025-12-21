using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.InsuranceCompany;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany
{
    public class UpdateInsuranceCompanyCommandHandler : IRequestHandler<UpdateInsuranceCompanyCommand, Result<InsuranceCompanyDto>>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateInsuranceCompanyCommandHandler> _logger;

        public UpdateInsuranceCompanyCommandHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<UpdateInsuranceCompanyCommandHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<InsuranceCompanyDto>> Handle(UpdateInsuranceCompanyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating insurance company with ID: {CompanyId}", request.Id);

                // Get existing company
                var company = await _insuranceCompanyRepository.GetByIdAsync(request.Id);
                if (company == null)
                {
                    return Result<InsuranceCompanyDto>.NotFound("Sigorta şirketi bulunamadı");
                }

                // Check if name is being changed and if new name already exists
                if (company.Name != request.CompanyDto.Name)
                {
                    var existingCompany = await _insuranceCompanyRepository.GetByNameAsync(request.CompanyDto.Name);
                    if (existingCompany != null && existingCompany.Id != request.Id)
                    {
                        return Result<InsuranceCompanyDto>.Conflict("Bu isimde bir sigorta şirketi zaten mevcut");
                    }
                }

                // Update properties
                company.Name = request.CompanyDto.Name;
                company.Description = request.CompanyDto.Description;
                company.Email = request.CompanyDto.ContactEmail ?? string.Empty;
                company.Phone = request.CompanyDto.ContactPhone ?? string.Empty;
                company.Address = request.CompanyDto.Address ?? string.Empty;
                company.Website = request.CompanyDto.Website;
                company.IsActive = request.CompanyDto.IsActive;
                company.UpdateAuditInfo(_currentUserService.UserName ?? "System");

                // Save changes
                _insuranceCompanyRepository.Update(company);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var companyDto = _mapper.Map<InsuranceCompanyDto>(company);

                _logger.LogInformation("Insurance company updated successfully: {CompanyId}", company.Id);

                return Result<InsuranceCompanyDto>.Success(companyDto, "Sigorta şirketi başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance company with ID: {CompanyId}", request.Id);
                return Result<InsuranceCompanyDto>.InternalError("Sigorta şirketi güncellenirken bir hata oluştu");
            }
        }
    }
}
