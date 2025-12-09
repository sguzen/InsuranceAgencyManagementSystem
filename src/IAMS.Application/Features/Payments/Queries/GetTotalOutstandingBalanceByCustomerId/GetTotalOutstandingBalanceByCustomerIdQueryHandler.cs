using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Payments.Queries.GetTotalOutstandingBalanceByCustomerId
{
    public class GetTotalOutstandingBalanceByCustomerIdQueryHandler : IRequestHandler<GetTotalOutstandingBalanceByCustomerIdQuery, decimal>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalOutstandingBalanceByCustomerIdQueryHandler> _logger;

        public GetTotalOutstandingBalanceByCustomerIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalOutstandingBalanceByCustomerIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<decimal> Handle(GetTotalOutstandingBalanceByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.PolicyPayments.GetTotalOutstandingBalanceByCustomerIdAsync(request.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total outstanding balance for customer {CustomerId}", request.CustomerId);
                throw;
            }
        }
    }
}
