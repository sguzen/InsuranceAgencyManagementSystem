# Logging Mechanism Improvements

## Overview

This document describes the comprehensive logging improvements implemented for the Insurance Agency Management System (IAMS). The logging system has been enhanced to support both production and development environments with proper request tracing, audit logging, and performance monitoring.

## What Was Implemented

### 1. Enhanced Serilog Configuration

#### New NuGet Packages Added (IAMS.Api.csproj)
- `Serilog.Enrichers.Environment` (3.1.0) - Machine name and environment enrichment
- `Serilog.Enrichers.Thread` (4.0.0) - Thread ID enrichment
- `Serilog.Formatting.Compact` (3.0.0) - Structured JSON logging
- `Serilog.Sinks.Async` (2.1.0) - Asynchronous logging for better performance
- `Microsoft.IO.RecyclableMemoryStream` (3.0.1) - Efficient memory management for HTTP logging

#### Infrastructure Package Updates (IAMS.Infrastructure.csproj)
- `Microsoft.AspNetCore.Http.Abstractions` (2.2.0) - For HTTP context access
- `Serilog` (4.1.0) - Core Serilog library

### 2. Custom Serilog Enrichers

Created three custom enrichers in `IAMS.Infrastructure/Logging/Enrichers/`:

#### TenantEnricher.cs
- Automatically adds tenant context to all logs
- Enriches with: `TenantId`, `TenantIdentifier`, `TenantName`
- Critical for multi-tenant isolation and debugging

#### UserEnricher.cs
- Adds authenticated user information to logs
- Enriches with: `UserId`, `Username`, `UserRoles`
- Useful for audit trails and security monitoring

#### CorrelationIdEnricher.cs
- Adds correlation ID for request tracing
- Enriches with: `CorrelationId`
- Enables tracking requests across the entire pipeline

### 3. Middleware Components

Created four new middleware components in `IAMS.Api/Middleware/`:

#### CorrelationIdMiddleware.cs
**Purpose:** Request tracing across the application

Features:
- Generates unique correlation ID for each request (or uses client-provided ID)
- Adds correlation ID to response headers (`X-Correlation-ID`)
- Stores in HttpContext.Items for access throughout request pipeline
- Pushes to Serilog LogContext for automatic inclusion in all logs

#### HttpLoggingMiddleware.cs
**Purpose:** Detailed HTTP request/response logging

Features:
- Logs all HTTP requests with method, path, query string, content type
- Logs HTTP responses with status code, duration, content type
- Request/response body logging (configurable, size-limited)
- Excludes health check endpoints to reduce noise
- Uses RecyclableMemoryStream for efficient memory usage
- Log level varies based on response status (Error for 5xx, Warning for 4xx)

#### ExceptionHandlingMiddleware.cs
**Purpose:** Centralized exception handling and logging

Features:
- Catches all unhandled exceptions
- Maps exceptions to appropriate HTTP status codes
- Logs exceptions with full context (correlation ID, tenant, user, path)
- Returns structured error responses with:
  - StatusCode, Message, ErrorCode
  - TraceId and CorrelationId for debugging
  - Detailed stack traces in development mode only
- Handles application-specific exceptions (NotFoundException, ValidationException, TenantException)

#### PerformanceLoggingMiddleware.cs
**Purpose:** Performance monitoring and slow request detection

Features:
- Tracks request duration and memory usage
- Logs slow requests as warnings (configurable threshold, default: 3000ms)
- Includes tenant information in performance logs
- Provides metrics for monitoring and optimization
- Helps identify performance bottlenecks per tenant

### 4. Audit Logging Service

Created comprehensive audit logging in `IAMS.Infrastructure/`:

#### IAuditLogger.cs (Interface)
Defines audit logging operations:
- `LogAsync()` - Generic audit log
- `LogActionAsync()` - CRUD operations
- `LogAuthenticationAsync()` - Login/logout events
- `LogAuthorizationAsync()` - Access control decisions
- `LogDataAccessAsync()` - Sensitive data access
- `LogConfigurationChangeAsync()` - System configuration changes

#### AuditLogger.cs (Implementation)
Features:
- Automatically enriches with user, tenant, correlation ID, IP address, user agent
- Structured logging with full context
- Logs both summary and detailed JSON for analysis
- Designed for compliance and security auditing

#### AuditLog Model
Comprehensive audit log entry with:
- Timestamp, User information, Tenant information
- Action, EntityType, EntityId
- Old/New values for change tracking
- CorrelationId, IP address, User agent
- Additional metadata dictionary

### 5. Configuration Updates

#### appsettings.json (Base Configuration)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Information",
        "System": "Warning",
        "IAMS": "Debug"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithThreadId"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "File",
              "Args": {
                "path": "logs/iams-.txt",
                "rollingInterval": "Day",
                "retainedFileCountLimit": 30
              }
            }
          ]
        }
      },
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "File",
              "Args": {
                "path": "logs/iams-json-.json",
                "rollingInterval": "Day",
                "retainedFileCountLimit": 30,
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter"
              }
            }
          ]
        }
      }
    ]
  },
  "Logging": {
    "SlowRequestThresholdMs": 3000,
    "EnableHttpLogging": true,
    "EnablePerformanceLogging": true
  }
}
```

#### appsettings.Development.json
- Log level: `Debug` for detailed debugging
- Includes EF Core command logging
- Console output with tenant/user/correlation context
- 7-day log retention
- Slow request threshold: 2000ms
- HTTP logging: Enabled
- Performance logging: Enabled

#### appsettings.Production.json
- Log level: `Warning` for reduced noise (Information for IAMS namespace)
- Structured JSON console output for log aggregation
- 90-day log retention with file size limits (100MB per file)
- Slow request threshold: 5000ms
- HTTP logging: Disabled (to reduce overhead)
- Performance logging: Enabled

### 6. Program.cs Updates

#### Serilog Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "IAMS.Api")
    .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
    .CreateLogger();
```

#### Service Registration
```csharp
// Register Audit Logger
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
```

#### Middleware Pipeline Order (Critical!)
```csharp
// 1. Exception handling - must be first to catch all exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Correlation ID - establish request tracing early
app.UseMiddleware<CorrelationIdMiddleware>();

// 3. Performance logging - track full request lifecycle
app.UseMiddleware<PerformanceLoggingMiddleware>();

// 4. HTTP request/response logging (conditionally based on configuration)
var enableHttpLogging = builder.Configuration.GetValue<bool>("Logging:EnableHttpLogging", true);
if (enableHttpLogging)
{
    app.UseMiddleware<HttpLoggingMiddleware>();
}

// 5. Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");

// 6. Multi-tenancy middleware - establish tenant context before authentication
app.UseMultiTenancy();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Application endpoints
app.MapControllers();
```

## Key Features

### Multi-Tenant Logging
- Every log entry automatically includes tenant context
- Each tenant's database is isolated, and logs clearly identify which tenant
- Tenant ID, Identifier, and Name are included in structured logs
- Critical for debugging issues in specific tenants

### Request Tracing
- Correlation IDs enable tracking a single request through the entire system
- Client can provide correlation ID or system generates one
- Correlation ID is returned in response headers
- All logs for a request share the same correlation ID

### Structured Logging
- JSON formatted logs for production (easy parsing and aggregation)
- Compact JSON format reduces storage requirements
- Human-readable format for development
- All logs include rich context (machine, environment, thread, app version)

### Performance Monitoring
- Automatic tracking of request duration and memory usage
- Slow request detection with configurable thresholds
- Per-tenant performance metrics
- Helps identify performance issues before they impact users

### Audit Trail
- Comprehensive audit logging for compliance
- Tracks authentication, authorization, data access, configuration changes
- Immutable audit logs with full context
- Suitable for security analysis and regulatory compliance

### Error Handling
- Centralized exception handling
- Consistent error responses
- Full error context logging
- Development vs Production error detail levels

## Log File Structure

### Development
- `logs/iams-dev-YYYYMMDD.txt` - Human-readable logs with full context

### Production
- `logs/iams-YYYYMMDD.txt` - Human-readable logs
- `logs/iams-json-YYYYMMDD.json` - Structured JSON logs for aggregation tools

## Usage Examples

### Using Audit Logger in Services

```csharp
public class PolicyService
{
    private readonly IAuditLogger _auditLogger;

    public PolicyService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task<Policy> CreatePolicyAsync(CreatePolicyDto dto)
    {
        var policy = // ... create policy

        // Audit the creation
        await _auditLogger.LogActionAsync(
            action: "CREATE",
            entityType: "Policy",
            entityId: policy.Id.ToString(),
            newValues: policy,
            description: $"Created new {policy.PolicyType} policy"
        );

        return policy;
    }
}
```

### Accessing Correlation ID

```csharp
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        // Use for external API calls, etc.
        return Ok();
    }
}
```

## Configuration Options

### Logging Configuration Keys

| Key | Default | Description |
|-----|---------|-------------|
| `Logging:SlowRequestThresholdMs` | 3000 | Milliseconds before request is logged as slow |
| `Logging:EnableHttpLogging` | true | Enable detailed HTTP request/response logging |
| `Logging:EnablePerformanceLogging` | true | Enable performance metrics logging |

### Serilog Log Levels

| Environment | Default | Microsoft | System | IAMS | EF Core |
|-------------|---------|-----------|--------|------|---------|
| Development | Debug | Warning | Warning | Debug | Information |
| Production | Warning | Error | Error | Information | Warning |

## Benefits for Production

1. **Debugging**: Correlation IDs and rich context make issue diagnosis faster
2. **Performance**: Async logging and configurable levels reduce overhead
3. **Compliance**: Audit logging provides comprehensive activity trail
4. **Multi-Tenancy**: Clear tenant isolation in logs prevents confusion
5. **Monitoring**: Structured JSON logs integrate with log aggregation tools
6. **Security**: Authentication/authorization events are tracked
7. **Cost Control**: Log retention policies and file size limits

## Next Steps

1. **Log Aggregation**: Consider integrating with ELK Stack, Seq, or Azure Application Insights
2. **Alerting**: Set up alerts for slow requests, errors, and security events
3. **Dashboard**: Create dashboards for key metrics (requests/sec, error rates, slow queries)
4. **Retention Policy**: Adjust log retention based on compliance requirements
5. **Custom Sinks**: Add database sink for critical audit logs if needed

## Files Modified/Created

### Created
- `src/IAMS.Api/Middleware/CorrelationIdMiddleware.cs`
- `src/IAMS.Api/Middleware/HttpLoggingMiddleware.cs`
- `src/IAMS.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `src/IAMS.Api/Middleware/PerformanceLoggingMiddleware.cs`
- `src/IAMS.Api/appsettings.Development.json`
- `src/IAMS.Api/appsettings.Production.json`
- `src/IAMS.Infrastructure/Logging/Enrichers/TenantEnricher.cs`
- `src/IAMS.Infrastructure/Logging/Enrichers/UserEnricher.cs`
- `src/IAMS.Infrastructure/Logging/Enrichers/CorrelationIdEnricher.cs`
- `src/IAMS.Infrastructure/Interfaces/IAuditLogger.cs`
- `src/IAMS.Infrastructure/Services/AuditLogger.cs`

### Modified
- `src/IAMS.Api/IAMS.Api.csproj` - Added NuGet packages
- `src/IAMS.Api/Program.cs` - Configured Serilog and middleware pipeline
- `src/IAMS.Api/appsettings.json` - Updated Serilog configuration
- `src/IAMS.Infrastructure/IAMS.Infrastructure.csproj` - Added dependencies

## Technical Notes

- All middleware is registered in the correct order to ensure proper operation
- The tenant middleware runs before authentication to establish tenant context
- Exception handling middleware runs first to catch all errors
- Correlation ID middleware runs early to establish tracing
- HTTP logging is optional and can be disabled in production for performance
- All enrichers access HttpContext through IHttpContextAccessor
- Audit logging is scoped to match the HTTP request lifecycle
