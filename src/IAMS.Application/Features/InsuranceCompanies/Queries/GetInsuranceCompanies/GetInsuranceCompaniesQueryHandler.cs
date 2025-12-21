using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.InsuranceCompany;

namespace IAMS.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies
{
    public class GetInsuranceCompaniesQueryHandler : IRequestHandler<GetInsuranceCompaniesQuery, Result<PagedResult<InsuranceCompanyDto>>>
    {
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetInsuranceCompaniesQueryHandler> _logger;

        public GetInsuranceCompaniesQueryHandler(
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IMapper mapper,
            ILogger<GetInsuranceCompaniesQueryHandler> logger)
        {
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<InsuranceCompanyDto>>> Handle(GetInsuranceCompaniesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting insurance companies with query params");

                // Get all companies (would need to be enhanced with proper filtering in repository)
                var allCompanies = await _insuranceCompanyRepository.GetAllAsync();

                // Apply filters
                var query = allCompanies.AsQueryable();

                // Filter by search term (Name)
                if (!string.IsNullOrWhiteSpace(request.QueryParams.SearchTerm))
                {
                    var searchTerm = request.QueryParams.SearchTerm.ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(searchTerm) ||
                                           (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
                }

                // Filter by IsActive
                if (request.QueryParams.IsActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == request.QueryParams.IsActive.Value);
                }

                // Filter by date range
                if (request.QueryParams.CreatedFrom.HasValue)
                {
                    query = query.Where(c => c.CreatedOn >= request.QueryParams.CreatedFrom.Value);
                }

                if (request.QueryParams.CreatedTo.HasValue)
                {
                    query = query.Where(c => c.CreatedOn <= request.QueryParams.CreatedTo.Value);
                }

                // Filter by HasActivePolicies
                if (request.QueryParams.HasActivePolicies.HasValue && request.QueryParams.HasActivePolicies.Value)
                {
                    query = query.Where(c => c.Policies != null &&
                                           c.Policies.Any(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted));
                }

                // Get total count before pagination
                var totalCount = query.Count();

                // Apply sorting (default by Name)
                query = query.OrderBy(c => c.Name);

                // Apply pagination
                var pageNumber = request.QueryParams.PageNumber < 1 ? 1 : request.QueryParams.PageNumber;
                var pageSize = request.QueryParams.PageSize < 1 ? 10 : request.QueryParams.PageSize;

                var companies = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Map to DTOs
                var companyDtos = _mapper.Map<List<InsuranceCompanyDto>>(companies);

                // Enhance DTOs with aggregated data
                foreach (var dto in companyDtos)
                {
                    var company = companies.First(c => c.Id == dto.Id);
                    dto.ActivePoliciesCount = company.Policies?.Count(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted) ?? 0;
                    dto.TotalPolicies = company.Policies?.Count(p => !p.IsDeleted) ?? 0;
                    dto.TotalPremiums = company.Policies?.Where(p => !p.IsDeleted).Sum(p => p.PremiumAmount) ?? 0;
                    dto.TotalCommissions = company.Policies?.Where(p => !p.IsDeleted).Sum(p => p.CommissionAmount) ?? 0;
                    var deletedPolcies = company.Policies?.Where(p => p.IsDeleted).Count() ?? 0;
                    dto.LastPolicyDate = deletedPolcies >0 ? company.Policies?.Where(p => !p.IsDeleted).Max(p => p.CreatedOn) : null;
                }

                var pagedResult = new PagedResult<InsuranceCompanyDto>(
                    companyDtos,
                    totalCount,
                    pageNumber,
                    pageSize
                );

                return Result<PagedResult<InsuranceCompanyDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving insurance companies");
                return Result<PagedResult<InsuranceCompanyDto>>.InternalError("An error occurred while retrieving insurance companies");
            }
        }
    }
}
