using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Payment
{
    public class PolicyPaymentDto
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? Reference { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}