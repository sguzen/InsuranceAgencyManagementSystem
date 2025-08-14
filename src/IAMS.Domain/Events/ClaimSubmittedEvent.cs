using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class ClaimSubmittedEvent : DomainEvent
    {
        public PolicyClaim Claim { get; }
        public string SubmittedBy { get; }

        public ClaimSubmittedEvent(PolicyClaim claim, string submittedBy) : base()
        {
            Claim = claim;
            SubmittedBy = submittedBy;
        }
    }
}