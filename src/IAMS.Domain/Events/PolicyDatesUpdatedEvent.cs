using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class PolicyDatesUpdatedEvent : DomainEvent
    {
        public Policy Policy { get; }
        public string UpdatedBy { get; }
        public DateTime OldStartDate { get; }
        public DateTime OldEndDate { get; }
        public DateTime NewStartDate { get; }
        public DateTime NewEndDate { get; }

        public PolicyDatesUpdatedEvent(Policy policy, string updatedBy,
            DateTime oldStartDate, DateTime oldEndDate,
            DateTime newStartDate, DateTime newEndDate)
            : base()
        {
            Policy = policy;
            UpdatedBy = updatedBy;
            OldStartDate = oldStartDate;
            OldEndDate = oldEndDate;
            NewStartDate = newStartDate;
            NewEndDate = newEndDate;
        }
    }
}