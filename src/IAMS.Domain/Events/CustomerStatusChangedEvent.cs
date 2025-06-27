using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class CustomerStatusChangedEvent
    {
        public Customer Customer { get; }
        public CustomerStatus OldStatus { get; }
        public CustomerStatus NewStatus { get; }
        public string ChangedBy { get; }

        public CustomerStatusChangedEvent(Customer customer, CustomerStatus oldStatus, CustomerStatus newStatus, string changedBy)
        {
            Customer = customer;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedBy = changedBy;
        }
    }
}
