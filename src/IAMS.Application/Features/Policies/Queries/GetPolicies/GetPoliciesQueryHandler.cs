using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, Result<PagedResult<PolicyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPoliciesQueryHandler> _logger;

        public GetPoliciesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPoliciesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<PolicyDto>>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedResult = await _unitOfWork.Policies.GetPoliciesPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

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
                return Result<PagedResult<PolicyDto>>.Failure("An error occurred while retrieving policies");
            }
        }
    }
}