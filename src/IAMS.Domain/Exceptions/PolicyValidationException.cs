using IAMS.Domain.Entities;

namespace IAMS.Domain.Exceptions
{
    public class PolicyValidationException : DomainException
    {
        public Policy? Policy { get; }
        public IReadOnlyList<string> ValidationErrors { get; }

        public PolicyValidationException(string message)
            : base("POLICY_VALIDATION_ERROR", message)
        {
            ValidationErrors = new List<string>();
        }

        public PolicyValidationException(Policy policy, IEnumerable<string> validationErrors)
            : base("POLICY_VALIDATION_ERROR", "Policy validation failed")
        {
            Policy = policy;
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        public PolicyValidationException(string message, IEnumerable<string> validationErrors)
            : base("POLICY_VALIDATION_ERROR", message)
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }
    }
}