namespace IAMS.Application.Exceptions
{
    public class TenantException : ApplicationException
    {
        public TenantException() : base("Tenant-related error occurred.") { }

        public TenantException(string message) : base(message) { }

        public TenantException(string message, Exception innerException) : base(message, innerException) { }
    }
}