using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetExpiringPoliciesCount
{
    public class GetExpiringPoliciesCountQueryHandler : IRequestHandler<GetExpiringPoliciesCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetExpiringPoliciesCountQueryHandler> _logger;

        public GetExpiringPoliciesCountQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetExpiringPoliciesCountQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetExpiringPoliciesCountQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var count = await _unitOfWork.Policies.GetExpiringPoliciesCountAsync(request.DaysAhead);

                _logger.LogDebug("Retrieved expiring policies count {Count} for tenant", count);

                return Result<int>.Success(count, $"{request.DaysAhead} gün içinde süresi dolacak {count} poliçe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring policies count");
                return Result<int>.InternalError("Süresi dolacak poliçe sayısı alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}