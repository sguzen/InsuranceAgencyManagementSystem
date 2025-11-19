# Exception Handling Best Practices

## Problem
The codebase currently has **301 generic exception handlers** across 154 files using:
```csharp
catch (Exception ex)
{
    // Generic handling loses specific error context
}
```

This pattern:
- Loses specific error context
- Makes debugging difficult
- Hides actual problems
- Provides poor error diagnostics

## Solution: Specific Exception Handling

### Pattern 1: Catch Specific Exceptions First

**❌ Bad:**
```csharp
try
{
    await _dbContext.SaveChangesAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error saving changes");
    return Result.Failure("An error occurred");
}
```

**✅ Good:**
```csharp
try
{
    await _dbContext.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict for entity {EntityType}", entityType);
    return Result.Failure("The record was modified by another user. Please refresh and try again.");
}
catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
{
    _logger.LogWarning(ex, "Duplicate key violation");
    return Result.Failure("A record with this identifier already exists.");
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database update failed for entity {EntityType}", entityType);
    return Result.Failure("Failed to save changes to the database.");
}
catch (OperationCanceledException)
{
    _logger.LogInformation("Operation was cancelled");
    throw; // Re-throw cancellation
}
// Only catch Exception as last resort for truly unexpected errors
catch (Exception ex)
{
    _logger.LogCritical(ex, "Unexpected error in {Operation}", nameof(SaveChanges));
    throw; // Or return appropriate error
}
```

### Pattern 2: Use Result Pattern Instead of Generic Catch

**✅ Best Practice:**
```csharp
public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
{
    try
    {
        // Validation
        if (string.IsNullOrEmpty(request.FirstName))
            return Result<CustomerDto>.Failure("First name is required");

        // Business logic
        var customer = new Customer(request.FirstName, request.LastName);

        await _repository.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogWarning(ex, "Concurrency conflict creating customer");
        return Result<CustomerDto>.Failure("The record was modified by another process.");
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error creating customer");
        return Result<CustomerDto>.Failure("Failed to create customer due to database error.");
    }
    // Let unexpected exceptions bubble up to global exception handler
}
```

### Pattern 3: Domain-Specific Exceptions

**Create custom exceptions:**
```csharp
public class PolicyNotFoundException : Exception
{
    public int PolicyId { get; }

    public PolicyNotFoundException(int policyId)
        : base($"Policy with ID {policyId} was not found.")
    {
        PolicyId = policyId;
    }
}

public class PolicyAlreadyCancelledException : Exception
{
    public int PolicyId { get; }

    public PolicyAlreadyCancelledException(int policyId)
        : base($"Policy {policyId} is already cancelled.")
    {
        PolicyId = policyId;
    }
}
```

**Use them:**
```csharp
public async Task<Result<PolicyDto>> Handle(CancelPolicyCommand request, CancellationToken cancellationToken)
{
    try
    {
        var policy = await _repository.GetByIdAsync(request.PolicyId);

        if (policy == null)
            throw new PolicyNotFoundException(request.PolicyId);

        if (policy.Status == PolicyStatus.Cancelled)
            throw new PolicyAlreadyCancelledException(request.PolicyId);

        policy.Cancel(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PolicyDto>.Success(_mapper.Map<PolicyDto>(policy));
    }
    catch (PolicyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Policy not found: {PolicyId}", ex.PolicyId);
        return Result<PolicyDto>.Failure($"Policy {ex.PolicyId} not found.");
    }
    catch (PolicyAlreadyCancelledException ex)
    {
        _logger.LogWarning(ex, "Policy already cancelled: {PolicyId}", ex.PolicyId);
        return Result<PolicyDto>.Failure($"Policy {ex.PolicyId} is already cancelled.");
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error cancelling policy {PolicyId}", request.PolicyId);
        return Result<PolicyDto>.Failure("Failed to cancel policy due to database error.");
    }
}
```

### Pattern 4: Middleware for Global Exception Handling

**Create a global exception handler:**
```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (PolicyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Policy not found");
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            await WriteErrorResponse(context, StatusCodes.Status403Forbidden, "Access denied");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict");
            await WriteErrorResponse(context, StatusCodes.Status409Conflict,
                "The record was modified by another user.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception");
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message, statusCode };
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Common Exception Types to Catch

1. **Database Exceptions:**
   - `DbUpdateConcurrencyException` - Concurrency conflicts
   - `DbUpdateException` - General database update errors
   - `SqlException` - SQL Server specific errors

2. **Validation Exceptions:**
   - `ValidationException` - FluentValidation
   - `ArgumentNullException` - Null arguments
   - `ArgumentException` - Invalid arguments

3. **Business Logic Exceptions:**
   - Custom domain exceptions (e.g., `PolicyNotFoundException`)
   - `InvalidOperationException` - Invalid state

4. **Infrastructure Exceptions:**
   - `HttpRequestException` - HTTP calls
   - `TimeoutException` - Timeouts
   - `OperationCanceledException` - Cancellation

5. **Authorization:**
   - `UnauthorizedAccessException` - Authorization failures

## Migration Strategy

To fix the 301 generic exception handlers:

1. **Priority 1 (Critical):** Command handlers (Create, Update, Delete operations)
2. **Priority 2 (High):** Query handlers (Read operations)
3. **Priority 3 (Medium):** Services and repositories
4. **Priority 4 (Low):** Background services and utilities

### Example Refactoring

**Before:**
```csharp
try
{
    var policy = await _repository.GetByIdAsync(id);
    return Result.Success(policy);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting policy");
    return Result.Failure("Error occurred");
}
```

**After:**
```csharp
try
{
    var policy = await _repository.GetByIdAsync(id);

    if (policy == null)
        return Result<PolicyDto>.Failure($"Policy {id} not found");

    return Result<PolicyDto>.Success(_mapper.Map<PolicyDto>(policy));
}
catch (OperationCanceledException)
{
    _logger.LogInformation("Get policy operation cancelled");
    throw;
}
// Let other exceptions bubble to global handler
```

## Summary

✅ **DO:**
- Catch specific exceptions
- Use custom domain exceptions
- Log with context
- Use Result pattern for expected failures
- Implement global exception handler
- Re-throw cancellation tokens

❌ **DON'T:**
- Use generic `catch (Exception)` unless absolutely necessary
- Swallow exceptions silently
- Return generic error messages
- Catch exceptions you can't handle
- Log and re-throw (pick one)

## Next Steps

1. Create custom exception types for domain logic
2. Implement global exception handler middleware
3. Refactor command handlers (highest priority)
4. Update query handlers
5. Remove unnecessary try-catch blocks that don't add value
