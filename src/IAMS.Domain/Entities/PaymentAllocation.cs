using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Links a customer payment to a specific policy (allocation)
    /// </summary>
    public class PaymentAllocation : BaseEntity
    {
        public int CustomerPaymentId { get; set; }
        public int PolicyId { get; set; }

        /// <summary>
        /// Amount allocated from customer payment to this policy
        /// </summary>
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// When the allocation was made
        /// </summary>
        public DateTime AllocationDate { get; set; }

        /// <summary>
        /// How the allocation was made (Automatic FIFO, Manual)
        /// </summary>
        public AllocationType AllocationType { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public virtual CustomerPayment CustomerPayment { get; set; } = null!;
        public virtual Policy Policy { get; set; } = null!;
    }
}
