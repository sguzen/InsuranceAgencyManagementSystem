using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Payment
{
    /// <summary>
    /// Lightweight DTO for payment list operations
    /// Optimized for list views - only essential fields
    /// </summary>
    public class PolicyPaymentListDto
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public string? Reference { get; set; }
    }
}
