using AutoMapper;
using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, Result<PagedResult<PolicyDto>>>
    {
        private readonly IPolicyQueryService _policyQueryService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPoliciesQueryHandler> _logger;

        public GetPoliciesQueryHandler(
            IPolicyQueryService policyQueryService,
            IMapper mapper,
            ILogger<GetPoliciesQueryHandler> logger)
        {
            _policyQueryService = policyQueryService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<PolicyDto>>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedResult = await _policyQueryService.GetPoliciesAsync(request.QueryParams);

                var policyDtos = new PagedResult<PolicyDto>
                {
                    Items = _mapper.Map<List<PolicyDto>>(pagedResult.Items),
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                return Result<PagedResult<PolicyDto>>.Success(policyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policies");
                return Result<PagedResult<PolicyDto>>.InternalError("An error occurred while retrieving policies");
            }
        }
    }
}