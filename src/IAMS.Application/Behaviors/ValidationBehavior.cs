using MediatR;
using FluentValidation;
using IAMS.Application.Models;

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

                    // If TResponse is a Result type, return a failure result
                    if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        var resultType = typeof(TResponse).GetGenericArguments()[0];
                        var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure", new[] { typeof(string), typeof(List<string>), typeof(int) });
                        return (TResponse)failureMethod!.Invoke(null, new object[] { "Validation failed", errors, 400 })!;
                    }
                    else if (typeof(TResponse) == typeof(Result))
                    {
                        return (TResponse)(object)Result.Failure("Validation failed", errors, 400);
                    }

                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }
}