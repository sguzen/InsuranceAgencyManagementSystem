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

        // Navigation properties
        public virtual Policy Policy { get; set; } = null!;
    }

    public enum PaymentMethod
    {
        Cash = 0,
        CreditCard = 1,
        BankTransfer = 2,
        Cheque = 3
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3
    }
}