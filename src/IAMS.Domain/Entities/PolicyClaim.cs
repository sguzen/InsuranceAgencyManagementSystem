namespace IAMS.Domain.Entities
{
    public class PolicyClaim : BaseEntity
    {
        public int PolicyId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public decimal? SettledAmount { get; set; }
        public ClaimStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Policy Policy { get; set; } = null!;
    }

    public enum ClaimStatus
    {
        Submitted = 0,
        UnderReview = 1,
        Approved = 2,
        Rejected = 3,
        Settled = 4
    }
}
