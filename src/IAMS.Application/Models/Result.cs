namespace IAMS.Application.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; protected set; } = string.Empty;
        public string? ErrorMessage => IsFailure ? Message : null;
        public List<string> Errors { get; protected set; } = new();
        public List<string>? ValidationErrors => Errors?.Any() == true ? Errors : null;
        public T? Data { get; protected set; }
        public int StatusCode { get; protected set; } = 200;

        protected Result(bool isSuccess, string message, T? data = default, List<string>? errors = null, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        public static Result<T> Success(T data, string message = "Operation completed successfully")
        {
            return new Result<T>(true, message, data, null, 200);
        }

        public static Result<T> Success(string message = "Operation completed successfully")
        {
            return new Result<T>(true, message, default, null, 200);
        }

        public static Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors, 400);
        }

        public static Result<T> Failure(string message, string error)
        {
            return new Result<T>(false, message, default, new List<string> { error }, 400);
        }

        public static Result<T> Failure(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new Result<T>(false, message, default, errors, statusCode);
        }

        public static Result<T> ValidationFailure(string message, List<string> validationErrors)
        {
            return new Result<T>(false, message, default, validationErrors, 422);
        }

        public static Result<T> NotFound(string message = "Resource not found")
        {
            return new Result<T>(false, message, default, null, 404);
        }

        public static Result<T> Unauthorized(string message = "Unauthorized access")
        {
            return new Result<T>(false, message, default, null, 401);
        }

        public static Result<T> Forbidden(string message = "Access forbidden")
        {
            return new Result<T>(false, message, default, null, 403);
        }

        public static Result<T> InternalError(string message = "Internal server error", List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors, 500);
        }

        public static Result<T> Conflict(string message, List<string>? errors = null)
        {
            return new Result<T>(false, message, default, errors, 409);
        }

        // Implicit conversion operators for easier usage
        public static implicit operator bool(Result<T> result) => result.IsSuccess;

        // Helper method to get first error
        public string? GetFirstError() => Errors?.FirstOrDefault();

        // Helper method to get all errors as a single string
        public string GetAllErrors(string separator = "; ") => string.Join(separator, Errors ?? new List<string>());
    }

    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; protected set; } = string.Empty;
        public string? ErrorMessage => IsFailure ? Message : null;
        public List<string> Errors { get; protected set; } = new();
        public List<string>? ValidationErrors => Errors?.Any() == true ? Errors : null;
        public int StatusCode { get; protected set; } = 200;

        protected Result(bool isSuccess, string message, List<string>? errors = null, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        public static Result Success(string message = "Operation completed successfully")
        {
            return new Result(true, message, null, 200);
        }

        public static Result Failure(string message, List<string>? errors = null)
        {
            return new Result(false, message, errors, 400);
        }

        public static Result Failure(string message, string error)
        {
            return new Result(false, message, new List<string> { error }, 400);
        }

        public static Result Failure(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new Result(false, message, errors, statusCode);
        }

        public static Result ValidationFailure(string message, List<string> validationErrors)
        {
            return new Result(false, message, validationErrors, 422);
        }

        public static Result NotFound(string message = "Resource not found")
        {
            return new Result(false, message, null, 404);
        }

        public static Result Unauthorized(string message = "Unauthorized access")
        {
            return new Result(false, message, null, 401);
        }

        public static Result Forbidden(string message = "Access forbidden")
        {
            return new Result(false, message, null, 403);
        }

        public static Result InternalError(string message = "Internal server error", List<string>? errors = null)
        {
            return new Result(false, message, errors, 500);
        }

        public static Result Conflict(string message, List<string>? errors = null)
        {
            return new Result(false, message, errors, 409);
        }

        // Implicit conversion operators for easier usage
        public static implicit operator bool(Result result) => result.IsSuccess;

        // Helper method to get first error
        public string? GetFirstError() => Errors?.FirstOrDefault();

        // Helper method to get all errors as a single string
        public string GetAllErrors(string separator = "; ") => string.Join(separator, Errors ?? new List<string>());

        // Convert to generic Result<T>
        public Result<T> ToGeneric<T>(T? data = default)
        {
            return IsSuccess
                ? Result<T>.Success(data!, Message)
                : Result<T>.Failure(Message, Errors, StatusCode);
        }
    }
}