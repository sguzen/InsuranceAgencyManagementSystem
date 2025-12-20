using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Payment
{
    public class CreateCustomerPaymentDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public PaymentMethod PaymentMethod { get; set; }
        public int CurrencyId { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}
