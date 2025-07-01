using IAMS.Domain.Entities;
using IAMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyRenewedEvent : IDomainEvent
    {
        public Policy OriginalPolicy { get; }
        public Policy RenewalPolicy { get; }
        public string RenewedBy { get; }

        public DateTime OccurredOn => throw new NotImplementedException();

        public int TenantId => throw new NotImplementedException();

        public PolicyRenewedEvent(Policy originalPolicy, Policy renewalPolicy, string renewedBy)
        {
            OriginalPolicy = originalPolicy;
            RenewalPolicy = renewalPolicy;
            RenewedBy = renewedBy;
        }
    }
}
