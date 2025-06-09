namespace IAMS.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }
        public int TenantId { get; }

        protected DomainException(string code, string message, int tenantId = 0) : base(message)
        {
            Code = code;
            TenantId = tenantId;
        }

        protected DomainException(string code, string message, Exception innerException, int tenantId = 0)
            : base(message, innerException)
        {
            Code = code;
            TenantId = tenantId;
        }
    }
}