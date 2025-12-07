using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetTotalPaymentsByPolicyId
{
    public class GetTotalPaymentsByPolicyIdQueryHandler : IRequestHandler<GetTotalPaymentsByPolicyIdQuery, decimal>
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

        public async Task<decimal> Handle(GetTotalPaymentsByPolicyIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.PolicyPayments.GetTotalPaymentsByPolicyIdAsync(request.PolicyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total payments for policy ID: {PolicyId}", request.PolicyId);
                throw;
            }
        }
    }
}
