namespace IAMS.Domain.Enums
{
    /// <summary>
    /// Status of payment allocation to policies
    /// </summary>
    public enum AllocationStatus
    {
        /// <summary>
        /// Payment has not been allocated to any policy yet
        /// </summary>
        Unallocated = 0,

        /// <summary>
        /// Payment has been partially allocated (some amount remaining)
        /// </summary>
        PartiallyAllocated = 1,

        /// <summary>
        /// Payment has been fully allocated to policies
        /// </summary>
        FullyAllocated = 2
    }
}
