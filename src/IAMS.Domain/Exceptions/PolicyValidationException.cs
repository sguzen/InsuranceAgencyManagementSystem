using IAMS.Domain.Entities;

namespace IAMS.Domain.Exceptions
{
    public class PolicyValidationException : DomainException
    {
        public Policy? Policy { get; }
        public IReadOnlyList<string> ValidationErrors { get; }

        public PolicyValidationException(string message, int tenantId = 0)
            : base("POLICY_VALIDATION_ERROR", message, tenantId)
        {
            ValidationErrors = new List<string>();
        }

        public PolicyValidationException(Policy policy, IEnumerable<string> validationErrors)
            : base("POLICY_VALIDATION_ERROR", "Policy validation failed", policy?.TenantId ?? 0)
        {
            Policy = policy;
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }

        public PolicyValidationException(string message, IEnumerable<string> validationErrors, int tenantId = 0)
            : base("POLICY_VALIDATION_ERROR", message, tenantId)
        {
            ValidationErrors = validationErrors.ToList().AsReadOnly();
        }
    }
}