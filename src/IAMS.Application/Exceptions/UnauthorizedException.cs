namespace IAMS.Application.Exceptions
{
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException() : base("You are not authorized to perform this action.") { }

        public UnauthorizedException(string message) : base(message) { }
    }
}