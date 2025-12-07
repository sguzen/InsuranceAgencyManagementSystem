using AutoMapper;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetEndorsementsByPolicyId
{
    public class GetEndorsementsByPolicyIdQueryHandler : IRequestHandler<GetEndorsementsByPolicyIdQuery, Result<List<PolicyDto>>>
    {
        private readonly IPolicyQueryService _policyQueryService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEndorsementsByPolicyIdQueryHandler> _logger;

        public GetEndorsementsByPolicyIdQueryHandler(
            IPolicyQueryService policyQueryService,
            IMapper mapper,
            ILogger<GetEndorsementsByPolicyIdQueryHandler> logger)
        {
            _policyQueryService = policyQueryService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<PolicyDto>>> Handle(GetEndorsementsByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting endorsements for policy {PolicyId}", request.PolicyId);

                var endorsements = await _policyQueryService.GetEndorsementsByOriginalPolicyIdAsync(request.PolicyId);
                var endorsementDtos = _mapper.Map<List<PolicyDto>>(endorsements);

                _logger.LogInformation("Found {Count} endorsements for policy {PolicyId}", endorsementDtos.Count, request.PolicyId);

                return Result<List<PolicyDto>>.Success(endorsementDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting endorsements for policy {PolicyId}", request.PolicyId);
                return Result<List<PolicyDto>>.InternalError($"Zeyilnameler alınırken hata oluştu: {ex.Message}");
            }
        }
    }
}
