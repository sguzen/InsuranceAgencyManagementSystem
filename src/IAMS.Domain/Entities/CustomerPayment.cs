using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Represents a payment made by a customer (not tied to specific policy)
    /// </summary>
    public class CustomerPayment : BaseEntity
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int CurrencyId { get; set; }

        /// <summary>
        /// Exchange rate at time of payment (for historical accuracy)
        /// </summary>
        public decimal? ExchangeRateToBase { get; set; }

        /// <summary>
        /// Amount converted to base currency (TRY)
        /// </summary>
        public decimal? AmountInBaseCurrency { get; set; }

        /// <summary>
        /// Receipt or transaction reference number
        /// </summary>
        public string? Reference { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Payment status (Pending, Completed, Failed)
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// How much of this payment has been allocated to policies
        /// </summary>
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Remaining unallocated amount
        /// </summary>
        public decimal UnallocatedAmount => Amount - AllocatedAmount;

        /// <summary>
        /// Allocation status
        /// </summary>
        public AllocationStatus AllocationStatus { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();

        // Business methods
        public void MarkAsCompleted(string processedBy)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending payments can be marked as completed");

            Status = PaymentStatus.Completed;
            UpdateAuditInfo(processedBy);
        }

        public void AddAllocation(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Allocation amount must be positive", nameof(amount));

            if (AllocatedAmount + amount > Amount)
                throw new InvalidOperationException("Cannot allocate more than payment amount");

            AllocatedAmount += amount;

            // Update allocation status
            if (AllocatedAmount == 0)
                AllocationStatus = AllocationStatus.Unallocated;
            else if (AllocatedAmount < Amount)
                AllocationStatus = AllocationStatus.PartiallyAllocated;
            else
                AllocationStatus = AllocationStatus.FullyAllocated;
        }

        public bool CanAllocate(decimal amount)
        {
            return UnallocatedAmount >= amount && amount > 0;
        }
    }
}
