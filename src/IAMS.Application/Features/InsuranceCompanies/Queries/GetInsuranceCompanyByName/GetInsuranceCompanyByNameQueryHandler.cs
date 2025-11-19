using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanyByName
{
    public class GetInsuranceCompanyByNameQueryHandler : IRequestHandler<GetInsuranceCompanyByNameQuery, Result<InsuranceCompanyDto>>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInsuranceCompanyByNameQueryHandler> _logger;

        public GetInsuranceCompanyByNameQueryHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IMapper mapper,
            ILogger<GetInsuranceCompanyByNameQueryHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<InsuranceCompanyDto>> Handle(GetInsuranceCompanyByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting insurance company by name: {Name}", request.Name);

                var company = await _insuranceCompanyRepository.GetByNameAsync(request.Name);

                if (company == null)
                {
                    return Result<InsuranceCompanyDto>.NotFound("Sigorta şirketi bulunamadı");
                }

                var companyDto = _mapper.Map<InsuranceCompanyDto>(company);

                // Enhance DTO with aggregated data
                companyDto.ActivePoliciesCount = company.Policies?.Count(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted) ?? 0;
                companyDto.TotalPolicies = company.Policies?.Count(p => !p.IsDeleted) ?? 0;
                companyDto.TotalPremiums = company.Policies?.Where(p => !p.IsDeleted).Sum(p => p.PremiumAmount) ?? 0;
                companyDto.TotalCommissions = company.Policies?.Where(p => !p.IsDeleted).Sum(p => p.CommissionAmount) ?? 0;
                companyDto.LastPolicyDate = company.Policies?.Where(p => !p.IsDeleted).Max(p => p.CreatedOn);

                return Result<InsuranceCompanyDto>.Success(companyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance company by name: {Name}", request.Name);
                return Result<InsuranceCompanyDto>.InternalError("Sigorta şirketi getirilirken bir hata oluştu");
            }
        }
    }
}
