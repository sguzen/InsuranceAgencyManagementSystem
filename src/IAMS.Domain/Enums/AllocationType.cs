namespace IAMS.Domain.Enums
{
    /// <summary>
    /// Type of payment allocation to policy
    /// </summary>
    public enum AllocationType
    {
        /// <summary>
        /// Allocation was done automatically using FIFO strategy
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Allocation was done manually by user
        /// </summary>
        Manual = 1
    }
}
