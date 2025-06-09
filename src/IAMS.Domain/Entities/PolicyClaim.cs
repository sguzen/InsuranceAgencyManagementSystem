using IAMS.Domain.Enums;
using IAMS.Domain.ValueObjects;
using IAMS.Domain.Events;

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
        public ClaimType ClaimType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Currency { get; set; } = "TRY";

        // Navigation properties
        public virtual Policy Policy { get; set; } = null!;

        // Value objects
        public Money GetClaimMoney() => new Money(ClaimAmount, Currency);
        public Money? GetSettledMoney() => SettledAmount.HasValue ? new Money(SettledAmount.Value, Currency) : null;

        // Business methods
        public void SubmitClaim(string submittedBy)
        {
            if (Status != ClaimStatus.Submitted)
                throw new InvalidOperationException("Claim has already been submitted");

            Status = ClaimStatus.UnderReview;
            UpdateAuditInfo(submittedBy);
            AddDomainEvent(new ClaimSubmittedEvent(this, submittedBy));
        }

        public void ApproveClaim(string approvedBy)
        {
            if (Status != ClaimStatus.UnderReview)
                throw new InvalidOperationException("Only claims under review can be approved");

            Status = ClaimStatus.Approved;
            UpdateAuditInfo(approvedBy);
        }

        public void RejectClaim(string reason, string rejectedBy)
        {
            Status = ClaimStatus.Rejected;
            Notes = $"{Notes}\nRejected: {reason}".Trim();
            UpdateAuditInfo(rejectedBy);
        }

        public void SettleClaim(decimal settledAmount, string settledBy)
        {
            if (Status != ClaimStatus.Approved)
                throw new InvalidOperationException("Only approved claims can be settled");

            SettledAmount = settledAmount;
            Status = ClaimStatus.Settled;
            UpdateAuditInfo(settledBy);
            AddDomainEvent(new ClaimSettledEvent(this, new Money(settledAmount, Currency), settledBy));
        }

        public bool IsSettleable => Status == ClaimStatus.Approved;
        public bool IsActive => Status != ClaimStatus.Settled && Status != ClaimStatus.Rejected && Status != ClaimStatus.Closed;
    }
}