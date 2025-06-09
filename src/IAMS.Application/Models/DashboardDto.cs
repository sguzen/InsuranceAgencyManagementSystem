// src/IAMS.Application/Models/ApiResponse.cs
namespace IAMS.Application.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<T> FailureResult(string message, string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string message = "Operation completed successfully")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static new ApiResponse FailureResult(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static new ApiResponse FailureResult(string message, string error)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }
}

// src/IAMS.Application/Models/Result.cs
namespace IAMS.Application.Models
{
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
    }

    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        protected Result(bool isSuccess, T? value, string message, List<string>? errors = null)
            : base(isSuccess, message, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value, string message = "Operation completed successfully")
        {
            return new Result<T>(true, value, message);
        }

        public static new Result<T> Failure(string message, List<string>? errors = null)
        {
            return new Result<T>(false, default, message, errors);
        }

        public static new Result<T> Failure(string message, string error)
        {
            return new Result<T>(false, default, message, new List<string> { error });
        }
    }
}

// src/IAMS.Application/Models/DashboardDto.cs
namespace IAMS.Application.Models
{
    public class DashboardDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int ExpiringPoliciesThisMonth { get; set; }
        public decimal TotalPremiumAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public int PendingClaims { get; set; }
        public int OverduePayments { get; set; }
        public List<InsuranceCompanyStatsDto> InsuranceCompanyStats { get; set; } = new();
        public List<MonthlyStatsDto> MonthlyStats { get; set; } = new();
    }

    public class InsuranceCompanyStatsDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int PolicyCount { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommission { get; set; }
    }

    public class MonthlyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NewPolicies { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal TotalCommission { get; set; }
    }
}