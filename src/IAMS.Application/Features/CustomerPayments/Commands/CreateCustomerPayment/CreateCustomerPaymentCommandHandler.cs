using IAMS.Shared.DTOs.Payment;
using IAMS.Application.Interfaces;
using IAMS.Application.Services.Payments;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.CustomerPayments.Commands.CreateCustomerPayment
{
    public class CreateCustomerPaymentCommandHandler
        : IRequestHandler<CreateCustomerPaymentCommand, Result<CustomerPaymentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly PaymentAllocationService _allocationService;
        private readonly ILogger<CreateCustomerPaymentCommandHandler> _logger;

        public CreateCustomerPaymentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            PaymentAllocationService allocationService,
            ILogger<CreateCustomerPaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _allocationService = allocationService;
            _logger = logger;
        }

        public async Task<Result<CustomerPaymentDto>> Handle(
            CreateCustomerPaymentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.Payment;

                // Validate customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
                if (customer == null)
                {
                    return Result<CustomerPaymentDto>.Failure("Müşteri bulunamadı", (List<string>?)null);
                }

                // Validate currency exists
                var currency = await _unitOfWork.Currencies.GetByIdAsync(dto.CurrencyId);
                if (currency == null)
                {
                    return Result<CustomerPaymentDto>.Failure("Para birimi bulunamadı", (List<string>?)null);
                }

                // Validate amount
                if (dto.Amount <= 0)
                {
                    return Result<CustomerPaymentDto>.Failure("Ödeme tutarı sıfırdan büyük olmalıdır", (List<string>?)null);
                }

                // Create customer payment
                var payment = new CustomerPayment
                {
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    PaymentDate = dto.PaymentDate,
                    PaymentMethod = dto.PaymentMethod,
                    CurrencyId = dto.CurrencyId,
                    Reference = dto.Reference,
                    Notes = dto.Notes,
                    Status = PaymentStatus.Completed,
                    AllocationStatus = AllocationStatus.Unallocated,
                    AllocatedAmount = 0,
                    CreatedBy = _currentUserService.UserId ?? "System",
                    CreatedOn = DateTime.UtcNow
                };

                // Get exchange rate if needed
                if (currency.Code != "TRY")
                {
                    var exchangeRate = await _unitOfWork.CurrencyExchangeRates
                        .AsQueryable()
                        .Where(er => er.FromCurrencyId == dto.CurrencyId &&
                                    er.ToCurrency.Code == "TRY" &&
                                    er.EffectiveDate <= dto.PaymentDate &&
                                    (er.ExpiryDate == null || er.ExpiryDate >= dto.PaymentDate))
                        .OrderByDescending(er => er.EffectiveDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (exchangeRate != null)
                    {
                        payment.ExchangeRateToBase = exchangeRate.Rate;
                        payment.AmountInBaseCurrency = dto.Amount * exchangeRate.Rate;
                    }
                }
                else
                {
                    payment.ExchangeRateToBase = 1;
                    payment.AmountInBaseCurrency = dto.Amount;
                }

                // Save payment
                await _unitOfWork.CustomerPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created customer payment {PaymentId} for customer {CustomerId}, Amount: {Amount} {Currency}",
                    payment.Id, dto.CustomerId, dto.Amount, currency.Code);

                // Auto-allocate using FIFO
                try
                {
                    await _allocationService.AllocatePaymentAsync(
                        payment.Id,
                        _currentUserService.UserId ?? "System");

                    _logger.LogInformation("Auto-allocated payment {PaymentId}", payment.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to auto-allocate payment {PaymentId}, continuing...", payment.Id);
                    // Continue even if allocation fails - payment is still recorded
                }

                // Reload payment with allocations
                var savedPayment = await _unitOfWork.CustomerPayments.GetByIdWithRelationsAsync(payment.Id);

                var result = new CustomerPaymentDto
                {
                    Id = savedPayment!.Id,
                    CustomerId = savedPayment.CustomerId,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    Amount = savedPayment.Amount,
                    PaymentDate = savedPayment.PaymentDate,
                    PaymentMethod = savedPayment.PaymentMethod,
                    Reference = savedPayment.Reference,
                    Notes = savedPayment.Notes,
                    Status = savedPayment.Status,
                    AllocationStatus = savedPayment.AllocationStatus,
                    AllocatedAmount = savedPayment.AllocatedAmount,
                    UnallocatedAmount = savedPayment.UnallocatedAmount,
                    CurrencyId = savedPayment.CurrencyId,
                    CurrencyCode = currency.Code,
                    CurrencySymbol = currency.Symbol,
                    CreatedOn = savedPayment.CreatedOn
                };

                return Result<CustomerPaymentDto>.Success(result, "Ödeme başarıyla kaydedildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer payment");
                return Result<CustomerPaymentDto>.InternalError(
                    "Ödeme kaydedilirken bir hata oluştu",
                    new List<string> { ex.Message });
            }
        }
    }
}
