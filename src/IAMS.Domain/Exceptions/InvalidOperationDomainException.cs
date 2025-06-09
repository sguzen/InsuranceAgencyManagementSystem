namespace IAMS.Domain.Exceptions
{
    public class InvalidOperationDomainException : DomainException
    {
        public string Operation { get; }

        public InvalidOperationDomainException(string operation, string message, int tenantId = 0)
            : base("INVALID_OPERATION", message, tenantId)
        {
            Operation = operation;
        }

        public InvalidOperationDomainException(string operation, string message, Exception innerException, int tenantId = 0)
            : base("INVALID_OPERATION", message, innerException, tenantId)
        {
            Operation = operation;
        }
    }
}