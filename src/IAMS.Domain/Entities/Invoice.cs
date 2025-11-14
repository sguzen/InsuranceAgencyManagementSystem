using IAMS.Domain.Enums;
using IAMS.Domain.ValueObjects;

namespace IAMS.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int PolicyId { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxRate { get; set; } = 20; // Default VAT rate in Turkey
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime? PaymentDate { get; set; }
        public int CurrencyId { get; set; }
        public string? Notes { get; set; }
        public string? PaymentReference { get; set; }

        // Navigation properties
        public virtual Policy Policy { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        // Value objects
        public Money GetTotalMoney() => new Money(TotalAmount, CurrencyId);
        public Money GetSubtotalMoney() => new Money(SubtotalAmount, CurrencyId);
        public Money GetTaxMoney() => new Money(TaxAmount, CurrencyId);

        // Business methods
        public void CalculateTotals()
        {
            SubtotalAmount = InvoiceItems.Sum(i => i.TotalAmount);
            TaxAmount = SubtotalAmount * (TaxRate / 100);
            TotalAmount = SubtotalAmount + TaxAmount;
        }

        public void MarkAsPaid(DateTime paymentDate, string paymentReference, string paidBy)
        {
            if (PaymentStatus == PaymentStatus.Completed)
                throw new InvalidOperationException("Invoice is already marked as paid");

            PaymentStatus = PaymentStatus.Completed;
            PaymentDate = paymentDate;
            PaymentReference = paymentReference;
            UpdateAuditInfo(paidBy);
        }

        public void MarkAsOverdue(string updatedBy)
        {
            if (PaymentStatus != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending invoices can be marked as overdue");

            if (DueDate >= DateTime.Today)
                throw new InvalidOperationException("Invoice is not yet overdue");

            PaymentStatus = PaymentStatus.Failed; // Using Failed status for overdue
            UpdateAuditInfo(updatedBy);
        }

        public void CancelInvoice(string cancelledBy, string? reason = null)
        {
            if (PaymentStatus == PaymentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a paid invoice");

            PaymentStatus = PaymentStatus.Failed;
            Notes = string.IsNullOrEmpty(reason)
                ? Notes
                : $"{Notes}\nCancelled: {reason}".Trim();
            UpdateAuditInfo(cancelledBy);
        }

        public bool IsOverdue => PaymentStatus == PaymentStatus.Pending && DueDate < DateTime.Today;
        public bool IsPaid => PaymentStatus == PaymentStatus.Completed;
        public int DaysOverdue => IsOverdue ? (DateTime.Today - DueDate).Days : 0;
    }
}
