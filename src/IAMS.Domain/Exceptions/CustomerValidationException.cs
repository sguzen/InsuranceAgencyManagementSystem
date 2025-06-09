using IAMS.Domain.Entities;

namespace IAMS.Domain.Exceptions
{
    public class CustomerValidationException : DomainException
    {
        public Customer? Customer { get; }
        public IReadOnlyList<string> ValidationErrors { get; }

        public CustomerValidationException(string message, int tenantId = 0)
            : base("CUSTOMER_VALIDATION_ERROR", message, tenantId)
        {
            ValidationErrors = new List<string>();
        }

        public CustomerValidationException(Customer customer, IEnumerable<string> validationErrors)
            : base("CUSTOMER_VALIDATION_ERROR", "Customer validation failed", customer?.TenantId ?? 0)
        {
            Customer = customer;
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        public CustomerValidationException(string message, IEnumerable<string> validationErrors, int tenantId = 0)
            : base("CUSTOMER_VALIDATION_ERROR", message, tenantId)
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }
    }
}
