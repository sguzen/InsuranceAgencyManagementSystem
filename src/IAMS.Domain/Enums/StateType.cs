namespace IAMS.Domain.Enums
{
    /// <summary>
    /// Tip - Policy/Endorsement State Type (Column 3: TIP)
    /// </summary>
    public enum StateType
    {
        /// <summary>
        /// P - Yeni Police (New Policy) - Default for InnerCode 000
        /// </summary>
        YeniPolice = 0,

        /// <summary>
        /// T - Tecdit (Renewal)
        /// </summary>
        Tecdit = 1,

        /// <summary>
        /// V - Ilave (Addition/Supplementary)
        /// </summary>
        Ilave = 2,

        /// <summary>
        /// R - Iade (Return/Refund)
        /// </summary>
        Iade = 3,

        /// <summary>
        /// X - Iptal (Cancellation)
        /// </summary>
        Iptal = 4,

        /// <summary>
        /// Y - Yeniden (Re-issued/Again)
        /// </summary>
        Yeniden = 5
    }
}
