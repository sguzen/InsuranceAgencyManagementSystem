using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Application.DTOs.InsuranceCompany;

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

                // Map from value objects to DTOs
                var currencyBreakdowns = await _insuranceCompanyRepository.GetCurrencyBreakdownAsync(company.Id);
                companyDto.CurrencyBreakdowns = currencyBreakdowns.Select(cb => new CurrencyBreakdownDto
                {
                    CurrencyId = cb.CurrencyId,
                    CurrencyCode = cb.CurrencyCode,
                    CurrencySymbol = cb.CurrencySymbol,
                    TotalPremiums = cb.TotalPremiums,
                    TotalCommissions = cb.TotalCommissions,
                    PolicyCount = cb.PolicyCount
                }).ToList();

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