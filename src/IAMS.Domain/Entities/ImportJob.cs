namespace IAMS.Domain.Entities
{
    /// <summary>
    /// Tracks policy import jobs from external insurance company databases.
    /// Jobs are queued and processed by background workers.
    /// </summary>
    public class ImportJob : BaseEntity
    {
        /// <summary>
        /// The agency (tenant) ID this job belongs to.
        /// Used for credential lookup in master database.
        /// </summary>
        public int AgencyId { get; set; }

        public int InsuranceCompanyId { get; set; }

        /// <summary>
        /// Reference to the master InsuranceCompany ID for credential lookup.
        /// </summary>
        public int MasterInsuranceCompanyId { get; set; }

        public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Number of records found in source database.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Number of records successfully imported.
        /// </summary>
        public int ImportedRecords { get; set; }

        /// <summary>
        /// Number of records that failed to import.
        /// </summary>
        public int FailedRecords { get; set; }

        /// <summary>
        /// Number of records skipped (already exist, etc.).
        /// </summary>
        public int SkippedRecords { get; set; }

        /// <summary>
        /// Error message if the job failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Detailed log of the import process (JSON).
        /// </summary>
        public string? ImportLog { get; set; }

        /// <summary>
        /// Optional date range filter - start date.
        /// </summary>
        public DateTime? FilterStartDate { get; set; }

        /// <summary>
        /// Optional date range filter - end date.
        /// </summary>
        public DateTime? FilterEndDate { get; set; }

        /// <summary>
        /// User who requested the import.
        /// </summary>
        public string RequestedBy { get; set; } = string.Empty;

        // Navigation
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;

        // Computed properties
        public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
            ? CompletedAt.Value - StartedAt.Value
            : null;

        public double SuccessRate => TotalRecords > 0
            ? (double)ImportedRecords / TotalRecords * 100
            : 0;

        // Methods
        public void Start()
        {
            Status = ImportJobStatus.Running;
            StartedAt = DateTime.UtcNow;
        }

        public void Complete(int imported, int failed, int skipped)
        {
            Status = ImportJobStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ImportedRecords = imported;
            FailedRecords = failed;
            SkippedRecords = skipped;
            TotalRecords = imported + failed + skipped;
        }

        public void Fail(string errorMessage)
        {
            Status = ImportJobStatus.Failed;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
        }

        public void Cancel(string reason)
        {
            Status = ImportJobStatus.Cancelled;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = reason;
        }
    }

    public enum ImportJobStatus
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
