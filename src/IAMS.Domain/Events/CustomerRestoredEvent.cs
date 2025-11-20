using IAMS.Domain.Entities;
using IAMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class CustomerRestoredEvent : IDomainEvent
    {
        public Customer Customer { get; }
        public string RestoredBy { get; }

        public DateTime OccurredOn { get; }

        public CustomerRestoredEvent(Customer customer, string restoredBy)
        {
            Customer = customer;
            RestoredBy = restoredBy;
            OccurredOn = DateTime.UtcNow;
        }
    }
}