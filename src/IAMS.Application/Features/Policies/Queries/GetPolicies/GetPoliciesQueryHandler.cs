using MediatR;
using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, Result<PagedResult<PolicyDto>>>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPoliciesQueryHandler> _logger;

        public GetPoliciesQueryHandler(
            IPolicyRepository policyRepository,
            IMapper mapper,
            ILogger<GetPoliciesQueryHandler> logger)
        {
            _policyRepository = policyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<PolicyDto>>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting policies with parameters: Page: {PageNumber}, Size: {PageSize}",
                    request.QueryParams.PageNumber, request.QueryParams.PageSize);

                var (policies, totalCount) = await _policyRepository.GetPagedAsync(request.QueryParams);

                var policyDtos = _mapper.Map<List<PolicyDto>>(policies);

                // Add calculated properties
                foreach (var policyDto in policyDtos)
                {
                    var policy = policies.First(p => p.Id == policyDto.Id);
                    policyDto.OutstandingAmount = policy.GetOutstandingAmount();
                }

                var pagedResult = PagedResult<PolicyDto>.Create(
                    policyDtos,
                    totalCount,
                    request.QueryParams.PageNumber,
                    request.QueryParams.PageSize);

                return Result<PagedResult<PolicyDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies");
                return Result<PagedResult<PolicyDto>>.InternalError("An error occurred while retrieving policies");
            }
        }
    }
}