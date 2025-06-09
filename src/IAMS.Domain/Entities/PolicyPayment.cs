using IAMS.Domain.Enums;
using IAMS.Domain.ValueObjects;
using IAMS.Domain.Events;

namespace IAMS.Domain.Entities
{
    public class PolicyPayment : BaseEntity
    {
        public int PolicyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Reference { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Notes { get; set; }
        public string Currency { get; set; } = "TRY";

        // Navigation properties
        public virtual Policy Policy { get; set; } = null!;

        // Value objects
        public Money GetPaymentMoney() => new Money(Amount, Currency);

        // Business methods
        public void MarkAsCompleted(string processedBy)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending payments can be marked as completed");

            Status = PaymentStatus.Completed;
            UpdateAuditInfo(processedBy);
            AddDomainEvent(new PaymentProcessedEvent(this, processedBy));
        }

        public void MarkAsFailed(string reason, string processedBy)
        {
            Status = PaymentStatus.Failed;
            Notes = $"{Notes}\nFailed: {reason}".Trim();
            UpdateAuditInfo(processedBy);
        }

        public bool IsOverdue => Status == PaymentStatus.Pending && PaymentDate < DateTime.Today;
    }
}