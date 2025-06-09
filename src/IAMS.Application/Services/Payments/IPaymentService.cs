using IAMS.Application.DTOs.Payment;
using IAMS.Application.Models;
using IAMS.Domain.Enums;

namespace IAMS.Application.Services.Payments
{
    public interface IPaymentService
    {
        Task<List<PolicyPaymentDto>> GetPaymentsByPolicyIdAsync(int policyId);
        Task<PolicyPaymentDto?> GetByIdAsync(int id);
        Task<PolicyPaymentDto> CreateAsync(CreatePolicyPaymentDto paymentDto);
        Task UpdatePaymentStatusAsync(int id, PaymentStatus status);
        Task DeleteAsync(int id);
        Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync();
        Task<PagedResult<PolicyPaymentDto>> GetPaymentsPagedAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<decimal> GetTotalPaymentsByPolicyIdAsync(int policyId);
    }
}