using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetPoliciesByStatus
{
    public class GetPoliciesByStatusQueryHandler : IRequestHandler<GetPoliciesByStatusQuery, Result<Dictionary<PolicyStatus, int>>>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly ILogger<GetPoliciesByStatusQueryHandler> _logger;

        public GetPoliciesByStatusQueryHandler(
            IPolicyRepository policyRepository,
            ILogger<GetPoliciesByStatusQueryHandler> logger)
        {
            _policyRepository = policyRepository;
            _logger = logger;
        }


        public async Task<Result<Dictionary<PolicyStatus, int>>> Handle(GetPoliciesByStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var policyCountByStatus = await _policyRepository.GetPolicyCountByStatusAsync();

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
