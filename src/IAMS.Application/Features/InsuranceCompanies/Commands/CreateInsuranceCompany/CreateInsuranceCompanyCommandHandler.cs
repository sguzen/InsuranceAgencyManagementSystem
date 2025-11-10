using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany
{
    public class CreateInsuranceCompanyCommandHandler : IRequestHandler<CreateInsuranceCompanyCommand, Result<InsuranceCompanyDto>>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateInsuranceCompanyCommandHandler> _logger;

        public CreateInsuranceCompanyCommandHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<CreateInsuranceCompanyCommandHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<InsuranceCompanyDto>> Handle(CreateInsuranceCompanyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating insurance company: {Name}", request.CompanyDto.Name);

                // Check if company name already exists
                var existingCompany = await _insuranceCompanyRepository.GetByNameAsync(request.CompanyDto.Name);
                if (existingCompany != null)
                {
                    return Result<InsuranceCompanyDto>.InternalError("Insurance company with this name already exists");
                }

                // Create insurance company entity
                var company = InsuranceCompany.Create(
                    request.CompanyDto.Name,
                    request.CompanyDto.Description,
                    request.CompanyDto.ContactEmail,
                    request.CompanyDto.ContactPhone,
                    request.CompanyDto.Address,
                    request.CompanyDto.Website,
                    _currentUserService.UserName ?? "System");

                // Save to database
                await _insuranceCompanyRepository.AddAsync(company);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return
                var companyDto = _mapper.Map<InsuranceCompanyDto>(company);

                _logger.LogInformation("Insurance company created successfully with ID: {CompanyId}", company.Id);

                return Result<InsuranceCompanyDto>.Success(companyDto, "Insurance company created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating insurance company: {Name}", request.CompanyDto.Name);
                return Result<InsuranceCompanyDto>.InternalError("An error occurred while creating the insurance company");
            }
        }
    }
}