using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetTotalPoliciesCount
{
    public class GetTotalPoliciesCountQueryHandler : IRequestHandler<GetTotalPoliciesCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalPoliciesCountQueryHandler> _logger;

        public GetTotalPoliciesCountQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalPoliciesCountQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetTotalPoliciesCountQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var count = await _unitOfWork.Policies.GetPolicyCountAsync();

                _logger.LogDebug("Retrieved policy count {Count} for tenant {TenantId}", count);

                return Result<int>.Success(count, $"Toplam {count} poliçe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting policy count");
                return Result<int>.InternalError("Poliçe sayısı alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}