using IAMS.Domain.Entities;

namespace IAMS.Domain.Exceptions
{
    public class CustomerValidationException : DomainException
    {
        public Customer? Customer { get; }
        public IReadOnlyList<string> ValidationErrors { get; }

        public CustomerValidationException(string message)
            : base("CUSTOMER_VALIDATION_ERROR", message)
        {
            ValidationErrors = new List<string>();
        }

        public CustomerValidationException(Customer customer, IEnumerable<string> validationErrors)
            : base("CUSTOMER_VALIDATION_ERROR", "Customer validation failed")
        {
            Customer = customer;
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        public CustomerValidationException(string message, IEnumerable<string> validationErrors)
            : base("CUSTOMER_VALIDATION_ERROR", message)
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }
    }
}
