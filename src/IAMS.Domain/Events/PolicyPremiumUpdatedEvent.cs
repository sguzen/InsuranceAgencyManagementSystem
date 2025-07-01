using IAMS.Domain.Entities;
using IAMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyPremiumUpdatedEvent : IDomainEvent
    {
        public Policy Policy { get; }
        public decimal OldPremium { get; }
        public decimal NewPremium { get; }
        public string UpdatedBy { get; }

        public DateTime OccurredOn => throw new NotImplementedException();

        public int TenantId => throw new NotImplementedException();

        public PolicyPremiumUpdatedEvent(Policy policy, decimal oldPremium, decimal newPremium, string updatedBy)
        {
            Policy = policy;
            OldPremium = oldPremium;
            NewPremium = newPremium;
            UpdatedBy = updatedBy;
        }
    }
}