namespace IAMS.Application.Exceptions
{
    public class ConflictException : ApplicationException
    {
        public ConflictException() : base() { }

        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}