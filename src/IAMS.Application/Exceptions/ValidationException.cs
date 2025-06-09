namespace IAMS.Application.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public List<string> Errors { get; }

        public ValidationException() : base("One or more validation errors occurred.")
        {
            Errors = new List<string>();
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(List<string> errors) : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public ValidationException(string message, List<string> errors) : base(message)
        {
            Errors = errors;
        }
    }
}