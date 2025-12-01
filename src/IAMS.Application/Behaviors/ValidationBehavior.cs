using MediatR;
using FluentValidation;
using IAMS.Application.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace IAMS.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();

                    // Optimized: Use cached helper to minimize reflection overhead
                    var result = ValidationResultHelper.CreateValidationFailure<TResponse>(errors);
                    if (result != null)
                    {
                        return result;
                    }

                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }

    /// <summary>
    /// Helper class to create validation failure results with cached reflection
    /// Caching eliminates reflection overhead on subsequent calls
    /// </summary>
    internal static class ValidationResultHelper
    {
        // Cache delegates to eliminate reflection on repeated calls
        private static readonly ConcurrentDictionary<Type, Func<List<string>, object>> _factoryCache = new();

        public static TResponse? CreateValidationFailure<TResponse>(List<string> errors)
            where TResponse : class
        {
            // Fast path for non-generic Result
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure("Validation failed", errors);
            }

            // Check if TResponse is Result<T> (generic)
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Get or create cached factory delegate - reflection only happens once per type
                var factory = _factoryCache.GetOrAdd(typeof(TResponse), responseType =>
                {
                    var dataType = responseType.GetGenericArguments()[0];
                    var method = typeof(ValidationResultHelper)
                        .GetMethod(nameof(CreateGenericFailure), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(dataType);

                    // Create compiled delegate for fast invocation
                    return (Func<List<string>, object>)Delegate.CreateDelegate(
                        typeof(Func<List<string>, object>),
                        method);
                });

                return (TResponse)factory(errors);
            }

            return null;
        }

        private static Result<T> CreateGenericFailure<T>(List<string> errors)
        {
            return Result<T>.Failure("Validation failed", errors, 400);
        }
    }
}
