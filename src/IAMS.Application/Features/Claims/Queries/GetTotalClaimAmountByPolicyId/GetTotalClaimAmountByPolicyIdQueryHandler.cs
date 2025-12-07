using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Claims.Queries.GetTotalClaimAmountByPolicyId
{
    public class GetTotalClaimAmountByPolicyIdQueryHandler : IRequestHandler<GetTotalClaimAmountByPolicyIdQuery, decimal>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalClaimAmountByPolicyIdQueryHandler> _logger;

        public GetTotalClaimAmountByPolicyIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalClaimAmountByPolicyIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<decimal> Handle(GetTotalClaimAmountByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.PolicyClaims.GetTotalClaimAmountByPolicyIdAsync(request.PolicyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total claim amount for policy ID: {PolicyId}", request.PolicyId);
                throw;
            }
        }
    }
}
