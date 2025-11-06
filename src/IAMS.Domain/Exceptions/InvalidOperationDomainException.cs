namespace IAMS.Domain.Exceptions
{
    public class InvalidOperationDomainException : DomainException
    {
        public string Operation { get; }

        public InvalidOperationDomainException(string operation, string message = null)
            : base("INVALID_OPERATION", message)
        {
            Operation = operation;
        }

        public InvalidOperationDomainException(string operation, string message, Exception innerException = null)
            : base("INVALID_OPERATION", message, innerException)
        {
            Operation = operation;
        }
    }
}