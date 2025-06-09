using IAMS.Domain.ValueObjects;

namespace IAMS.Domain.Events
{
    public class CommissionCalculatedEvent : DomainEvent
    {
        public int PolicyId { get; }
        public Money CommissionAmount { get; }
        public decimal CommissionRate { get; }
        public string CalculatedBy { get; }

        public CommissionCalculatedEvent(int tenantId, int policyId, Money commissionAmount, decimal commissionRate, string calculatedBy)
            : base(tenantId)
        {
            PolicyId = policyId;
            CommissionAmount = commissionAmount;
            CommissionRate = commissionRate;
            CalculatedBy = calculatedBy;
        }
    }
}