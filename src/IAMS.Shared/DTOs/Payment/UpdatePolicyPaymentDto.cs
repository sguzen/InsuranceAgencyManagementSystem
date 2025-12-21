using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Payment
{
    public class UpdatePolicyPaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? Reference { get; set; }
    }
}
