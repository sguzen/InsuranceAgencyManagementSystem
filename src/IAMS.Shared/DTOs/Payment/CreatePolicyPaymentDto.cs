using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Payment
{
    public class CreatePolicyPaymentDto
    {
        public int PolicyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Notes { get; set; }
        public string? Reference { get; set; }
        public int CurrencyId { get; set; }
    }
}