using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Payment
{
    public class CustomerPaymentDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public PaymentStatus Status { get; set; }
        public AllocationStatus AllocationStatus { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }

        // Currency info
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
    }
}
