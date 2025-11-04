using AutoMapper;
using IAMS.Application.DTOs.Payment;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PolicyPaymentDto>> GetPaymentsByPolicyIdAsync(int policyId)
        {
            try
            {
                var payments = await _unitOfWork.PolicyPayments.GetPaymentsByPolicyIdAsync(policyId);
                return _mapper.Map<List<PolicyPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for policy ID: {PolicyId}", policyId);
                throw;
            }
        }

        public async Task<PolicyPaymentDto?> GetByIdAsync(int id)
        {
            try
            {
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(id);
                return payment != null ? _mapper.Map<PolicyPaymentDto>(payment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<PolicyPaymentDto> CreateAsync(CreatePolicyPaymentDto paymentDto)
        {
            try
            {
                // Verify policy exists
                var policy = await _unitOfWork.Policies.GetByIdAsync(paymentDto.PolicyId);
                if (policy == null)
                {
                    throw new InvalidOperationException($"Policy with ID {paymentDto.PolicyId} not found.");
                }

                var payment = _mapper.Map<PolicyPayment>(paymentDto);
                payment.CreatedOn = DateTime.UtcNow;

                // Set default status if not provided
                if (payment.Status == default)
                {
                    payment.Status = PaymentStatus.Pending;
                }

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment created successfully with ID: {PaymentId}", payment.Id);
                return _mapper.Map<PolicyPaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        public async Task UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            try
            {
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(id);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment with ID {id} not found.");
                }

                payment.Status = status;
                payment.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.PolicyPayments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment status updated successfully for ID: {PaymentId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var payment = await _unitOfWork.PolicyPayments.GetByIdAsync(id);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment with ID {id} not found.");
                }

                _unitOfWork.PolicyPayments.Remove(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment deleted successfully with ID: {PaymentId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment with ID: {PaymentId}", id);
                throw;
            }
        }

        public async Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync()
        {
            try
            {
                var payments = await _unitOfWork.PolicyPayments.GetOverduePaymentsAsync();
                return _mapper.Map<List<PolicyPaymentDto>>(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue payments");
                throw;
            }
        }

        public async Task<PagedResult<PolicyPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            try
            {
                var query = (await _unitOfWork.PolicyPayments.GetAllAsync()).AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        (p.Reference != null && p.Reference.ToLower().Contains(searchTerm)) ||
                        (p.Notes != null && p.Notes.ToLower().Contains(searchTerm)) ||
                        p.PolicyId.ToString().Contains(searchTerm));
                }

                var totalCount = query.Count();
                var payments = query
                    .OrderByDescending(p => p.PaymentDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var paymentDtos = _mapper.Map<List<PolicyPaymentDto>>(payments);

                return PagedResult<PolicyPaymentDto>.Success(paymentDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged payments");
                return PagedResult<PolicyPaymentDto>.InternalError("Error retrieving payments", new List<string> { ex.Message }, pageNumber, pageSize);
            }
        }

        public async Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId)
        {
            try
            {
                return await _unitOfWork.PolicyPayments.GetTotalPaymentsByPolicyIdAsync(policyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total payments for policy ID: {PolicyId}", policyId);
                throw;
            }
        }
    }
}
