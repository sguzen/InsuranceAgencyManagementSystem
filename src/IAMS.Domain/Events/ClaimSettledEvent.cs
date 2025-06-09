using IAMS.Domain.Entities;
using IAMS.Domain.ValueObjects;

namespace IAMS.Domain.Events
{
    public class ClaimSettledEvent : DomainEvent
    {
        public PolicyClaim Claim { get; }
        public Money SettledAmount { get; }
        public string SettledBy { get; }
        public DateTime SettlementDate { get; }

        public ClaimSettledEvent(PolicyClaim claim, Money settledAmount, string settledBy)
            : base(claim.TenantId)
        {
            Claim = claim;
            SettledAmount = settledAmount;
            SettledBy = settledBy;
            SettlementDate = DateTime.UtcNow;
        }
    }
}