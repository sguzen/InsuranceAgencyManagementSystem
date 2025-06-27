using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyPremiumUpdatedEvent
    {
        public Policy Policy { get; }
        public decimal OldPremium { get; }
        public decimal NewPremium { get; }
        public string UpdatedBy { get; }

        public PolicyPremiumUpdatedEvent(Policy policy, decimal oldPremium, decimal newPremium, string updatedBy)
        {
            Policy = policy;
            OldPremium = oldPremium;
            NewPremium = newPremium;
            UpdatedBy = updatedBy;
        }
    }
}