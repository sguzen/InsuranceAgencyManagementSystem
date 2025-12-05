using IAMS.Domain.Enums;

namespace IAMS.Domain.Entities
{
    /// <summary>
    /// History/log of all import operations
    /// </summary>
    public class ImportHistory : BaseEntity
    {
        /// <summary>
        /// Import source type
        /// </summary>
        public ImportSourceType SourceType { get; set; }

        /// <summary>
        /// Import configuration used (null for Excel uploads)
        /// </summary>
        public int? ImportConfigurationId { get; set; }
        public virtual ImportConfiguration? ImportConfiguration { get; set; }

        /// <summary>
        /// Insurance company for imported policies
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        public virtual InsuranceCompany InsuranceCompany { get; set; } = null!;

        /// <summary>
        /// File name (for Excel uploads)
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// File size in bytes (for Excel uploads)
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Import start time
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Import completion time
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Import status (Success, Failed, PartialSuccess)
        /// </summary>
        public string Status { get; set; } = "InProgress";

        /// <summary>
        /// Total number of records found
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Number of successfully imported records
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of failed records
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Number of skipped records
        /// </summary>
        public int SkippedCount { get; set; }

        /// <summary>
        /// Error messages (JSON array)
        /// </summary>
        public string? ErrorMessages { get; set; }

        /// <summary>
        /// Additional notes or details
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// User who initiated the import
        /// </summary>
        public string ImportedBy { get; set; } = string.Empty;

        /// <summary>
        /// Duration in seconds
        /// </summary>
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Calculate duration
        /// </summary>
        public void CalculateDuration()
        {
            if (CompletedAt.HasValue)
            {
                DurationSeconds = (int)(CompletedAt.Value - StartedAt).TotalSeconds;
            }
        }

        /// <summary>
        /// Mark as completed
        /// </summary>
        public void MarkAsCompleted(string status)
        {
            CompletedAt = DateTime.UtcNow;
            Status = status;
            CalculateDuration();
        }
    }
}
