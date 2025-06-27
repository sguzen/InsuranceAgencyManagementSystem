using IAMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Domain.Events
{
    public class CustomerDeletedEvent
    {
        public Customer Customer { get; }
        public string DeletedBy { get; }

        public CustomerDeletedEvent(Customer customer, string deletedBy)
        {
            Customer = customer;
            DeletedBy = deletedBy;
        }
    }
}
