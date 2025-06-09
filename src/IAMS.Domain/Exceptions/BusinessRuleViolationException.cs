namespace IAMS.Domain.Exceptions
{
    public class BusinessRuleViolationException : DomainException
    {
        public string RuleName { get; }

        public BusinessRuleViolationException(string ruleName, string message, int tenantId = 0)
            : base("BUSINESS_RULE_VIOLATION", message, tenantId)
        {
            RuleName = ruleName;
        }

        public BusinessRuleViolationException(string ruleName, string message, Exception innerException, int tenantId = 0)
            : base("BUSINESS_RULE_VIOLATION", message, innerException, tenantId)
        {
            RuleName = ruleName;
        }
    }
}