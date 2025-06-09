using IAMS.Domain.Entities;

namespace IAMS.Domain.Events
{
    public class CustomerRegisteredEvent : DomainEvent
    {
        public Customer Customer { get; }
        public string RegisteredBy { get; }

        public CustomerRegisteredEvent(Customer customer, string registeredBy) : base(customer.TenantId)
        {
            Customer = customer;
            RegisteredBy = registeredBy;
        }
    }
}