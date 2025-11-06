using AutoMapper;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

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
                    var nonDeletedPolicies = company.Policies?.Where(p => !p.IsDeleted).ToList() ?? new List<Domain.Entities.Policy>();

                    dto.ActivePoliciesCount = nonDeletedPolicies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active);
                    dto.TotalPolicies = nonDeletedPolicies.Count;
                    dto.TotalPremiums = nonDeletedPolicies.Sum(p => p.PremiumAmount);
                    dto.TotalCommissions = nonDeletedPolicies.Sum(p => p.CommissionAmount);
                    dto.LastPolicyDate = nonDeletedPolicies.Any() ? nonDeletedPolicies.Max(p => p.CreatedOn) : null;
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
