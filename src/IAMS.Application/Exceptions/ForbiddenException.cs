namespace IAMS.Application.Exceptions
{
    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException() : base("Access to this resource is forbidden.") { }

        public ForbiddenException(string message) : base(message) { }
    }
}