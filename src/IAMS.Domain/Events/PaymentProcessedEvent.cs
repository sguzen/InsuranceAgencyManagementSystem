using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PaymentProcessedEvent : DomainEvent
    {
        public PolicyPayment Payment { get; }
        public string ProcessedBy { get; }

        public PaymentProcessedEvent(PolicyPayment payment, string processedBy) : base(payment.TenantId)
        {
            Payment = payment;
            ProcessedBy = processedBy;
        }
    }
}