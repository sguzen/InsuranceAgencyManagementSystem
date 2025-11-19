using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompany
{
    public class GetInsuranceCompanyQueryHandler : IRequestHandler<GetInsuranceCompanyQuery, Result<InsuranceCompanyDto>>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInsuranceCompanyQueryHandler> _logger;

        public GetInsuranceCompanyQueryHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IMapper mapper,
            ILogger<GetInsuranceCompanyQueryHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<InsuranceCompanyDto>> Handle(GetInsuranceCompanyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting insurance company with ID: {CompanyId}", request.Id);

                var company = await _insuranceCompanyRepository.GetByIdAsync(request.Id);
                if (company == null)
                {
                    return Result<InsuranceCompanyDto>.NotFound("Sigorta şirketi bulunamadı");
                }

                var companyDto = _mapper.Map<InsuranceCompanyDto>(company);

                // Add aggregated data
                companyDto.ActivePoliciesCount = await _insuranceCompanyRepository.GetActivePoliciesCountAsync(company.Id);
                companyDto.TotalPremiums = await _insuranceCompanyRepository.GetTotalPremiumAmountAsync(company.Id);
                companyDto.TotalCommissions = await _insuranceCompanyRepository.GetTotalCommissionsAsync(company.Id);

                return Result<InsuranceCompanyDto>.Success(companyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting insurance company with ID: {CompanyId}", request.Id);
                return Result<InsuranceCompanyDto>.InternalError("Sigorta şirketi getirilirken bir hata oluştu");
            }
        }
    }
}