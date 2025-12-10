using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
                // OPTIMIZED: Single database query with aggregation
                // Calculate outstanding balance directly in SQL
                var outstandingBalance = await _unitOfWork.Policies.GetAll()
                    .Where(p => !p.IsDeleted && p.CustomerId == request.CustomerId)
                    .SumAsync(p => p.PremiumAmount -
                        p.PolicyPayments
                            .Where(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Completed)
                            .Sum(pp => (decimal?)pp.Amount) ?? 0,
                        cancellationToken);

                return outstandingBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total outstanding balance for customer {CustomerId}", request.CustomerId);
                throw;
            }
        }
    }
}
