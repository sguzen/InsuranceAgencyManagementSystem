using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId
{
    public class GetTotalPaymentsByPolicyIdQueryHandler : IRequestHandler<GetTotalPaymentsByPolicyIdQuery, Result<decimal>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalPaymentsByPolicyIdQueryHandler> _logger;

        public GetTotalPaymentsByPolicyIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalPaymentsByPolicyIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(GetTotalPaymentsByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var total = await _unitOfWork.PolicyPayments.GetTotalPaymentsByPolicyIdAsync(request.PolicyId);
                return Result<decimal>.Success(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total payments for policy ID: {PolicyId}", request.PolicyId);
                return Result<decimal>.InternalError("Error calculating total payments", new List<string> { ex.Message });
            }
        }
    }
}
