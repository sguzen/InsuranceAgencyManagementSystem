using IAMS.Domain.Enums;

namespace IAMS.Shared.DTOs.Invoice
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxRate { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PaymentReference { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsPaid { get; set; }
        public int DaysOverdue { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new();
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
