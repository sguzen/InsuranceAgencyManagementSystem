namespace IAMS.Domain.Exceptions
{
    public class BusinessRuleViolationException : DomainException
    {
        public string RuleName { get; }

        public BusinessRuleViolationException(string ruleName, string message = null)
            : base("BUSINESS_RULE_VIOLATION", message)
        {
            RuleName = ruleName;
        }

        public BusinessRuleViolationException(string ruleName, string message, Exception innerException = null)
            : base("BUSINESS_RULE_VIOLATION", message, innerException)
        {
            RuleName = ruleName;
        }
    }
}