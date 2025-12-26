using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetBatchTotalPaymentsByPolicyIds
{
    public class GetBatchTotalPaymentsByPolicyIdsQueryHandler : IRequestHandler<GetBatchTotalPaymentsByPolicyIdsQuery, Result<Dictionary<int, decimal>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetBatchTotalPaymentsByPolicyIdsQueryHandler> _logger;

        public GetBatchTotalPaymentsByPolicyIdsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetBatchTotalPaymentsByPolicyIdsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Dictionary<int, decimal>>> Handle(GetBatchTotalPaymentsByPolicyIdsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.PolicyIds == null || !request.PolicyIds.Any())
                {
                    return Result<Dictionary<int, decimal>>.Success(new Dictionary<int, decimal>());
                }

                var totals = await _unitOfWork.PolicyPayments.GetTotalPaymentsByPolicyIdsAsync(request.PolicyIds);
                return Result<Dictionary<int, decimal>>.Success(totals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total payments for {Count} policies", request.PolicyIds?.Count ?? 0);
                return Result<Dictionary<int, decimal>>.InternalError("Error calculating batch total payments", new List<string> { ex.Message });
            }
        }
    }
}
