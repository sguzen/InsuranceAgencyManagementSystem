using IAMS.Application.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.CustomerPayments.Queries.GetCustomerPayments
{
    public class GetCustomerPaymentsQueryHandler
        : IRequestHandler<GetCustomerPaymentsQuery, Result<List<CustomerPaymentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCustomerPaymentsQueryHandler> _logger;

        public GetCustomerPaymentsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCustomerPaymentsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<CustomerPaymentDto>>> Handle(
            GetCustomerPaymentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return Result<List<CustomerPaymentDto>>.Failure("Müşteri bulunamadı", (List<string>?)null);
                }

                // Get all payments for this customer with currency info
                var payments = await _unitOfWork.CustomerPayments
                    .AsQueryable()
                    .Where(p => p.CustomerId == request.CustomerId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ThenByDescending(p => p.CreatedOn)
                    .ToListAsync(cancellationToken);

                var result = new List<CustomerPaymentDto>();

                foreach (var payment in payments)
                {
                    var currency = await _unitOfWork.Currencies.GetByIdAsync(payment.CurrencyId);

                    var dto = new CustomerPaymentDto
                    {
                        Id = payment.Id,
                        CustomerId = payment.CustomerId,
                        CustomerName = $"{customer.FirstName} {customer.LastName}",
                        Amount = payment.Amount,
                        PaymentDate = payment.PaymentDate,
                        PaymentMethod = payment.PaymentMethod,
                        Reference = payment.Reference,
                        Notes = payment.Notes,
                        Status = payment.Status,
                        AllocationStatus = payment.AllocationStatus,
                        AllocatedAmount = payment.AllocatedAmount,
                        UnallocatedAmount = payment.UnallocatedAmount,
                        CurrencyId = payment.CurrencyId,
                        CurrencyCode = currency?.Code ?? "",
                        CurrencySymbol = currency?.Symbol ?? "",
                        CreatedOn = payment.CreatedOn
                    };

                    result.Add(dto);
                }

                return Result<List<CustomerPaymentDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer payments for customer {CustomerId}", request.CustomerId);
                return Result<List<CustomerPaymentDto>>.InternalError(
                    "Müşteri ödemeleri getirilirken bir hata oluştu",
                    new List<string> { ex.Message });
            }
        }
    }
}
