namespace IAMS.Application.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Message { get; private set; }
        public List<string> Errors { get; private set; } = new();
        public int? StatusCode { get; private set; }

        protected Result(bool isSuccess, T? data, string? message, List<string>? errors = null, int? statusCode = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        public static Result<T> Success(T data, string? message = null, int statusCode = 200)
            => new(true, data, message, null, statusCode);

        public static Result<T> Failure(string message, List<string>? errors = null, int statusCode = 400)
            => new(false, default, message, errors, statusCode);

        public static Result<T> NotFound(string message = "Resource not found")
            => new(false, default, message, null, 404);

        public static Result<T> InternalError(string message = "Internal server error")
            => new(false, default, message, null, 500);
    }

    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? Message { get; private set; }
        public List<string> Errors { get; private set; } = new();
        public int? StatusCode { get; private set; }

        protected Result(bool isSuccess, string? message, List<string>? errors = null, int? statusCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        public static Result Success(string? message = null, int statusCode = 200)
            => new(true, message, null, statusCode);

        public static Result Failure(string message, List<string>? errors = null, int statusCode = 400)
            => new(false, message, errors, statusCode);

        public static Result NotFound(string message = "Resource not found")
            => new(false, message, null, 404);

        public static Result InternalError(string message = "Internal server error")
            => new(false, message, null, 500);
    }
}