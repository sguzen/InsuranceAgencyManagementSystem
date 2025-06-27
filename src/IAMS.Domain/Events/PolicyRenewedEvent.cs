using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class PolicyRenewedEvent
    {
        public Policy OriginalPolicy { get; }
        public Policy RenewalPolicy { get; }
        public string RenewedBy { get; }

        public PolicyRenewedEvent(Policy originalPolicy, Policy renewalPolicy, string renewedBy)
        {
            OriginalPolicy = originalPolicy;
            RenewalPolicy = renewalPolicy;
            RenewedBy = renewedBy;
        }
    }
}
