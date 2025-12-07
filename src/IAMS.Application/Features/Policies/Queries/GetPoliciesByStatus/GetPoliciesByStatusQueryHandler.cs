using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus
{
    public class GetPoliciesByStatusQueryHandler : IRequestHandler<GetPoliciesByStatusQuery, Result<Dictionary<PolicyStatus, int>>>
    {
        private readonly IPolicyAnalyticsService _policyAnalyticsService;
        private readonly ILogger<GetPoliciesByStatusQueryHandler> _logger;

        public GetPoliciesByStatusQueryHandler(
            IPolicyAnalyticsService policyAnalyticsService,
            ILogger<GetPoliciesByStatusQueryHandler> logger)
        {
            _policyAnalyticsService = policyAnalyticsService;
            _logger = logger;
        }


        public async Task<Result<Dictionary<PolicyStatus, int>>> Handle(GetPoliciesByStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var policyCountByStatus = await _policyAnalyticsService.GetPolicyCountByStatusAsync();

                if (policyCountByStatus == null || !policyCountByStatus.Any())
                {
                    return Result<Dictionary<PolicyStatus, int>>.NotFound("Poliçe bulunamadı");
                }

                return Result<Dictionary<PolicyStatus, int>>.Success(policyCountByStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policies by status");
                return Result<Dictionary<PolicyStatus, int>>.InternalError("Poliçe getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
