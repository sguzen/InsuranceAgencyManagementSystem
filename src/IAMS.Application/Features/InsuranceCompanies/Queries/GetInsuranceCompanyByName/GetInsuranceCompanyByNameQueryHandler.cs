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
                    return Result<InsuranceCompanyDto>.NotFound($"Insurance company with name '{request.Name}' not found");
                }

                var companyDto = _mapper.Map<InsuranceCompanyDto>(company);

                // Enhance DTO with aggregated data
                var nonDeletedPolicies = company.Policies?.Where(p => !p.IsDeleted).ToList() ?? new List<Domain.Entities.Policy>();

                companyDto.ActivePoliciesCount = nonDeletedPolicies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active);
                companyDto.TotalPolicies = nonDeletedPolicies.Count;
                companyDto.TotalPremiums = nonDeletedPolicies.Sum(p => p.PremiumAmount);
                companyDto.TotalCommissions = nonDeletedPolicies.Sum(p => p.CommissionAmount);
                companyDto.LastPolicyDate = nonDeletedPolicies.Any() ? nonDeletedPolicies.Max(p => p.CreatedOn) : null;

                return Result<InsuranceCompanyDto>.Success(companyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance company by name: {Name}", request.Name);
                return Result<InsuranceCompanyDto>.InternalError("An error occurred while retrieving the insurance company");
            }
        }
    }
}
