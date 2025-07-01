using IAMS.Domain.Entities;
using IAMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyDatesUpdatedEvent : IDomainEvent
    {
        public Policy Policy { get; }
        public string UpdatedBy { get; }

        public DateTime OccurredOn => throw new NotImplementedException();

        public int TenantId => throw new NotImplementedException();

        public PolicyDatesUpdatedEvent(Policy policy, string updatedBy)
        {
            Policy = policy;
            UpdatedBy = updatedBy;
        }
    }
}
