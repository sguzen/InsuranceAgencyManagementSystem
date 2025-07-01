namespace IAMS.Application.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; protected set; } = string.Empty;
        public List<string> Errors { get; protected set; } = new();
        public T? Data { get; protected set; }

        protected Result(bool isSuccess, string message, T? data = default, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> Success(T data, string message = "Operation completed successfully")
        {
            return new Result<T>(true, message, data);
        }

        public static Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors);
        }
        public static Result<T> InternalError(string message, List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors);
        }

        public static Result<T> Failure(string message, string error)
        {
            return new Result<T>(false, message, default, new List<string> { error });
        }

        public static Result<T> NotFound(string message = "Resource not found")
        {
            return new Result<T>(false, message);
        }
    }

    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; protected set; } = string.Empty;
        public List<string> Errors { get; protected set; } = new();

        protected Result(bool isSuccess, string message, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static Result Success(string message = "Operation completed successfully")
        {
            return new Result(true, message);
        }

        public static Result Failure(string message, List<string>? errors = null)
        {
            return new Result(false, message, errors);
        }

        public static Result Failure(string message, string error)
        {
            return new Result(false, message, new List<string> { error });
        }

        public static Result NotFound(string message = "Resource not found")
        {
            return new Result(false, message);
        }
    }
}
