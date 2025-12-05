namespace IAMS.Domain.Enums
{
    /// <summary>
    /// Source type for policy imports
    /// </summary>
    public enum ImportSourceType
    {
        /// <summary>
        /// Manual Excel file upload
        /// </summary>
        ExcelUpload = 0,

        /// <summary>
        /// API integration/sync from external system
        /// </summary>
        ApiSync = 1,

        /// <summary>
        /// Manual data entry
        /// </summary>
        ManualEntry = 2,

        /// <summary>
        /// Automated scheduled sync
        /// </summary>
        ScheduledSync = 3
    }
}
