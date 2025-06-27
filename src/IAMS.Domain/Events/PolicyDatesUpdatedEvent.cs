using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyDatesUpdatedEvent
    {
        public Policy Policy { get; }
        public string UpdatedBy { get; }

        public PolicyDatesUpdatedEvent(Policy policy, string updatedBy)
        {
            Policy = policy;
            UpdatedBy = updatedBy;
        }
    }
}
