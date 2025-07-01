using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class CustomerStatusChangedEvent : IDomainEvent
    {
        public Customer Customer { get; }
        public CustomerStatus OldStatus { get; }
        public CustomerStatus NewStatus { get; }
        public string ChangedBy { get; }

        public DateTime OccurredOn => throw new NotImplementedException();

        public int TenantId => throw new NotImplementedException();

        public CustomerStatusChangedEvent(Customer customer, CustomerStatus oldStatus, CustomerStatus newStatus, string changedBy)
        {
            Customer = customer;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedBy = changedBy;
        }
    }
}
