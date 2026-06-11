# Logging System Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [Usage Guide](#usage-guide)
6. [Middleware Components](#middleware-components)
7. [Audit Logging](#audit-logging)
8. [Log Enrichment](#log-enrichment)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)
11. [Production Deployment](#production-deployment)

---

## Overview

The Insurance Agency Management System (IAMS) uses a comprehensive logging infrastructure built on **Serilog** to provide:

- **Multi-tenant context awareness** - Every log entry includes tenant information
- **Request tracing** - Correlation IDs track requests across the entire pipeline
- **Performance monitoring** - Automatic tracking of slow requests and memory usage
- **Audit trails** - Comprehensive logging for compliance and security
- **Structured logging** - JSON formatted logs for easy parsing and analysis
- **Environment-specific behavior** - Different configurations for development and production

### Key Features

âœ… Automatic tenant, user, and request context enrichment
âœ… Correlation IDs for distributed tracing
âœ… Centralized exception handling with proper error responses
âœ… HTTP request/response logging
âœ… Performance metrics and slow request detection
âœ… Audit logging for compliance
âœ… Structured JSON output for log aggregation tools

---

## Architecture

### Components

```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚                      HTTP Request                            â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  1. ExceptionHandlingMiddleware                              â”‚
â”‚     â€¢ Catches all unhandled exceptions                       â”‚
â”‚     â€¢ Returns structured error responses                     â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  2. CorrelationIdMiddleware                                  â”‚
â”‚     â€¢ Generates/accepts correlation ID                       â”‚
â”‚     â€¢ Adds to LogContext and response headers                â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  3. PerformanceLoggingMiddleware                             â”‚
â”‚     â€¢ Tracks request duration                                â”‚
â”‚     â€¢ Monitors memory usage                                  â”‚
â”‚     â€¢ Logs slow requests                                     â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  4. HttpLoggingMiddleware (optional)                         â”‚
â”‚     â€¢ Logs HTTP requests/responses                           â”‚
â”‚     â€¢ Configurable detail level                              â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  5. TenantMiddleware                                         â”‚
â”‚     â€¢ Establishes tenant context                             â”‚
â”‚     â€¢ Adds tenant info to HttpContext.Items                  â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  Application Controllers & Services                          â”‚
â”‚     â€¢ Use ILogger<T> for logging                             â”‚
â”‚     â€¢ Use IAuditLogger for audit events                      â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  Serilog Enrichers                                           â”‚
â”‚     â€¢ TenantEnricher - Adds tenant context                   â”‚
â”‚     â€¢ UserEnricher - Adds user information                   â”‚
â”‚     â€¢ CorrelationIdEnricher - Adds correlation ID            â”‚
â”‚     â€¢ Environment, Machine, Thread enrichers                 â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
                            â†“
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  Serilog Sinks                                               â”‚
â”‚     â€¢ Console - Human-readable output                        â”‚
â”‚     â€¢ File - Text logs (logs/iams-YYYYMMDD.txt)             â”‚
â”‚     â€¢ File - JSON logs (logs/iams-json-YYYYMMDD.json)       â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Middleware Execution Order

The order is critical for proper operation:

1. **ExceptionHandlingMiddleware** - First to catch all exceptions
2. **CorrelationIdMiddleware** - Establish request tracing early
3. **PerformanceLoggingMiddleware** - Track full request lifecycle
4. **HttpLoggingMiddleware** - Log request/response (optional)
5. **TenantMiddleware** - Establish tenant context
6. **Authentication & Authorization** - Standard ASP.NET Core middleware
7. **Application Endpoints** - Your controllers

---

## Quick Start

### Basic Logging in Controllers

```csharp
using Microsoft.AspNetCore.Mvc;

namespace IAMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly ILogger<PoliciesController> _logger;

        public PoliciesController(ILogger<PoliciesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicy(int id)
        {
            _logger.LogInformation("Retrieving policy with ID: {PolicyId}", id);

            try
            {
                // Your logic here
                var policy = await GetPolicyById(id);

                _logger.LogInformation("Successfully retrieved policy {PolicyId}", id);
                return Ok(policy);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Policy not found: {PolicyId}", id);
                throw; // Exception middleware will handle it
            }
        }
    }
}
```

### Using Audit Logger

```csharp
using IAMS.Infrastructure.Interfaces;

public class PolicyService
{
    private readonly ILogger<PolicyService> _logger;
    private readonly IAuditLogger _auditLogger;

    public PolicyService(
        ILogger<PolicyService> logger,
        IAuditLogger auditLogger)
    {
        _logger = logger;
        _auditLogger = auditLogger;
    }

    public async Task<Policy> CreatePolicyAsync(CreatePolicyDto dto)
    {
        _logger.LogInformation("Creating new policy of type {PolicyType}", dto.PolicyType);

        var policy = MapToPolicy(dto);
        await _repository.AddAsync(policy);

        // Audit the creation
        await _auditLogger.LogActionAsync(
            action: "CREATE",
            entityType: "Policy",
            entityId: policy.Id.ToString(),
            newValues: policy,
            description: $"Created new {policy.PolicyType} policy for customer {policy.CustomerId}"
        );

        _logger.LogInformation("Policy created successfully with ID: {PolicyId}", policy.Id);
        return policy;
    }

    public async Task DeletePolicyAsync(int id)
    {
        var policy = await _repository.GetByIdAsync(id);

        // Log data access for sensitive operations
        await _auditLogger.LogDataAccessAsync("Policy", id.ToString(), "DELETE");

        await _repository.DeleteAsync(id);

        _logger.LogWarning("Policy deleted: {PolicyId}", id);
    }
}
```

---

## Configuration

### Environment Variables

```bash
# Development
ASPNETCORE_ENVIRONMENT=Development

# Production
ASPNETCORE_ENVIRONMENT=Production
```

### appsettings.json (Base Configuration)

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Async",
      "Serilog.Enrichers.Environment",
      "Serilog.Enrichers.Thread"
    ],
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
                "retainedFileCountLimit": 30,
                "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}"
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
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
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

### appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Information",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information",
        "System": "Warning",
        "IAMS": "Debug"
      }
    }
  },
  "Logging": {
    "SlowRequestThresholdMs": 2000,
    "EnableHttpLogging": true,
    "EnablePerformanceLogging": true
  }
}
```

### appsettings.Production.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Error",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Error",
        "IAMS": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
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
                "retainedFileCountLimit": 90,
                "fileSizeLimitBytes": 104857600,
                "rollOnFileSizeLimit": true
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
                "retainedFileCountLimit": 90,
                "fileSizeLimitBytes": 104857600,
                "rollOnFileSizeLimit": true,
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
              }
            }
          ]
        }
      }
    ]
  },
  "Logging": {
    "SlowRequestThresholdMs": 5000,
    "EnableHttpLogging": false,
    "EnablePerformanceLogging": true
  }
}
```

### Configuration Options

| Setting | Default | Description |
|---------|---------|-------------|
| `Logging:SlowRequestThresholdMs` | 3000 | Milliseconds before a request is logged as slow |
| `Logging:EnableHttpLogging` | true | Enable detailed HTTP request/response logging |
| `Logging:EnablePerformanceLogging` | true | Enable performance metrics logging |
| `Serilog:MinimumLevel:Default` | Information | Minimum log level for application logs |
| `Serilog:MinimumLevel:Override:Microsoft` | Warning | Log level for Microsoft framework logs |
| `Serilog:MinimumLevel:Override:IAMS` | Debug | Log level for IAMS application logs |

---

## Usage Guide

### Log Levels

Use appropriate log levels for different scenarios:

#### Debug
```csharp
_logger.LogDebug("Processing policy calculation with parameters: {@Parameters}", parameters);
```
- Detailed diagnostic information
- Only enabled in development
- Use for troubleshooting

#### Information
```csharp
_logger.LogInformation("Policy {PolicyId} created successfully for customer {CustomerId}",
    policy.Id, policy.CustomerId);
```
- Normal application flow
- Business process milestones
- Significant operations

#### Warning
```csharp
_logger.LogWarning("Policy {PolicyId} is approaching expiration date: {ExpiryDate}",
    policy.Id, policy.ExpiryDate);
```
- Unexpected but recoverable situations
- Business rule violations
- Deprecated API usage

#### Error
```csharp
try
{
    await ProcessPayment(payment);
}
catch (PaymentException ex)
{
    _logger.LogError(ex, "Payment processing failed for policy {PolicyId}. Amount: {Amount}",
        payment.PolicyId, payment.Amount);
    throw;
}
```
- Exceptions and errors
- Failed operations
- Includes exception details

#### Critical
```csharp
_logger.LogCritical("Database connection lost! Unable to process any requests.");
```
- System failures
- Data corruption
- Security breaches

### Structured Logging

Always use structured logging with named parameters:

**âœ… Good:**
```csharp
_logger.LogInformation("User {UserId} updated policy {PolicyId} with premium {Premium}",
    userId, policyId, premium);
```

**âŒ Bad:**
```csharp
_logger.LogInformation($"User {userId} updated policy {policyId} with premium {premium}");
```

**Why?** Structured logging allows you to:
- Query logs by specific fields
- Aggregate metrics
- Create dashboards
- Filter and search efficiently

### Using Correlation IDs

Correlation IDs are automatically added to every request. Access them in your code:

```csharp
public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<Response> CallExternalApiAsync()
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        // Pass correlation ID to external services
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/endpoint");
        request.Headers.Add("X-Correlation-ID", correlationId);

        return await _httpClient.SendAsync(request);
    }
}
```

### Scoped Logging Context

Add temporary context for a specific scope:

```csharp
using Serilog.Context;

public async Task ProcessBatchAsync(List<Policy> policies)
{
    var batchId = Guid.NewGuid();

    using (LogContext.PushProperty("BatchId", batchId))
    using (LogContext.PushProperty("BatchSize", policies.Count))
    {
        _logger.LogInformation("Starting batch processing");

        foreach (var policy in policies)
        {
            using (LogContext.PushProperty("PolicyId", policy.Id))
            {
                _logger.LogDebug("Processing policy");
                await ProcessPolicyAsync(policy);
            }
        }

        _logger.LogInformation("Batch processing completed");
    }
}
```

All logs within the scope will include `BatchId`, `BatchSize`, and `PolicyId`.

---

## Middleware Components

### 1. ExceptionHandlingMiddleware

Catches all unhandled exceptions and returns structured error responses.

**Location:** `src/IAMS.Api/Middleware/ExceptionHandlingMiddleware.cs`

**Features:**
- Maps exceptions to appropriate HTTP status codes
- Logs exceptions with full context
- Returns JSON error responses
- Includes detailed errors in development only

**Error Response Format:**
```json
{
  "statusCode": 404,
  "message": "Policy not found",
  "errorCode": "RESOURCE_NOT_FOUND",
  "traceId": "0HMVFE0J5N3QK:00000001",
  "correlationId": "a1b2c3d4e5f6",
  "timestamp": "2024-01-15T10:30:00Z",
  "details": "Policy with ID 123 was not found in the database",
  "stackTrace": "...",  // Development only
  "innerException": "..."  // Development only
}
```

**Exception Mapping:**

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `NotFoundException` | 404 | RESOURCE_NOT_FOUND |
| `ValidationException` | 400 | VALIDATION_ERROR |
| `UnauthorizedAccessException` | 401 | UNAUTHORIZED |
| `TenantException` | 400 | TENANT_ERROR |
| `InvalidOperationException` | 400 | INVALID_OPERATION |
| `ArgumentNullException` | 400 | MISSING_PARAMETER |
| `ArgumentException` | 400 | INVALID_ARGUMENT |
| Any other exception | 500 | INTERNAL_SERVER_ERROR |

### 2. CorrelationIdMiddleware

Generates or accepts correlation IDs for request tracing.

**Location:** `src/IAMS.Api/Middleware/CorrelationIdMiddleware.cs`

**Features:**
- Generates unique correlation ID for each request
- Accepts client-provided correlation ID via `X-Correlation-ID` header
- Adds correlation ID to response headers
- Pushes to LogContext for automatic inclusion in all logs

**Usage:**

Client can provide correlation ID:
```http
GET /api/policies/123
X-Correlation-ID: client-generated-id-12345
```

Response includes correlation ID:
```http
HTTP/1.1 200 OK
X-Correlation-ID: client-generated-id-12345
```

### 3. PerformanceLoggingMiddleware

Tracks request performance metrics.

**Location:** `src/IAMS.Api/Middleware/PerformanceLoggingMiddleware.cs`

**Features:**
- Measures request duration
- Tracks memory usage
- Logs slow requests as warnings
- Configurable threshold

**Configuration:**
```json
{
  "Logging": {
    "SlowRequestThresholdMs": 3000
  }
}
```

**Log Output:**
```
[10:30:15 WRN] SLOW REQUEST | GET /api/policies | Duration: 3250ms | Status: 200 | Memory: 1024KB | TenantId: agency1
```

### 4. HttpLoggingMiddleware

Logs HTTP requests and responses.

**Location:** `src/IAMS.Api/Middleware/HttpLoggingMiddleware.cs`

**Features:**
- Logs request method, path, query string
- Logs response status code and duration
- Optionally logs request/response bodies (size-limited)
- Excludes health check endpoints
- Configurable enable/disable

**Configuration:**
```json
{
  "Logging": {
    "EnableHttpLogging": true
  }
}
```

**Excluded Paths:**
- `/health`
- `/healthz`
- `/ready`
- `/live`
- `/swagger`
- `/favicon.ico`

---

## Audit Logging

### IAuditLogger Interface

The audit logger provides methods for logging business-critical events.

**Location:** `src/IAMS.Infrastructure/Interfaces/IAuditLogger.cs`

### Available Methods

#### 1. LogActionAsync
Log CRUD operations and business actions.

```csharp
await _auditLogger.LogActionAsync(
    action: "CREATE",
    entityType: "Policy",
    entityId: policy.Id.ToString(),
    newValues: policy,
    description: "Created new insurance policy"
);
```

#### 2. LogAuthenticationAsync
Log authentication events.

```csharp
await _auditLogger.LogAuthenticationAsync(
    userId: user.Id,
    username: user.Email,
    success: true,
    reason: "Login successful"
);
```

#### 3. LogAuthorizationAsync
Log authorization decisions.

```csharp
await _auditLogger.LogAuthorizationAsync(
    userId: currentUser.Id,
    resource: "Policy:123",
    action: "DELETE",
    granted: false,
    reason: "User does not have delete permission"
);
```

#### 4. LogDataAccessAsync
Log access to sensitive data.

```csharp
await _auditLogger.LogDataAccessAsync(
    entityType: "Customer",
    entityId: customerId.ToString(),
    action: "VIEW"
);
```

#### 5. LogConfigurationChangeAsync
Log system configuration changes.

```csharp
await _auditLogger.LogConfigurationChangeAsync(
    configKey: "MaxPolicyAmount",
    oldValue: 1000000,
    newValue: 2000000,
    reason: "Increased limit per management decision"
);
```

### Audit Log Entry Structure

```csharp
{
    "Timestamp": "2024-01-15T10:30:00Z",
    "UserId": "user123",
    "Username": "john.doe@agency.com",
    "TenantId": "1",
    "TenantIdentifier": "agency1",
    "Action": "CREATE",
    "EntityType": "Policy",
    "EntityId": "12345",
    "OldValues": { /* previous state */ },
    "NewValues": { /* new state */ },
    "Description": "Created new insurance policy",
    "CorrelationId": "a1b2c3d4e5f6",
    "IpAddress": "192.168.1.100",
    "UserAgent": "Mozilla/5.0...",
    "AdditionalData": { /* custom fields */ }
}
```

### Audit Logging Best Practices

1. **Always audit sensitive operations:**
   - User authentication/logout
   - Permission changes
   - Access to sensitive data (PII, financial)
   - Configuration changes
   - Policy creation/modification/deletion
   - Payment processing

2. **Include sufficient context:**
   ```csharp
   // âœ… Good - includes all relevant context
   await _auditLogger.LogActionAsync(
       action: "UPDATE",
       entityType: "Policy",
       entityId: policy.Id.ToString(),
       oldValues: originalPolicy,
       newValues: updatedPolicy,
       description: $"Updated premium from {originalPolicy.Premium} to {updatedPolicy.Premium}"
   );

   // âŒ Bad - missing context
   await _auditLogger.LogActionAsync(
       action: "UPDATE",
       entityType: "Policy",
       entityId: policy.Id.ToString()
   );
   ```

3. **Use descriptive action names:**
   - CREATE, UPDATE, DELETE
   - APPROVE, REJECT, CANCEL
   - EXPORT, IMPORT
   - ACTIVATE, DEACTIVATE

---

## Log Enrichment

### Automatic Enrichers

All logs are automatically enriched with the following properties:

| Property | Source | Description |
|----------|--------|-------------|
| `TenantId` | TenantEnricher | Tenant's database ID |
| `TenantIdentifier` | TenantEnricher | Tenant's unique identifier (subdomain) |
| `TenantName` | TenantEnricher | Tenant's display name |
| `UserId` | UserEnricher | Authenticated user's ID |
| `Username` | UserEnricher | Authenticated user's email/name |
| `UserRoles` | UserEnricher | User's assigned roles |
| `CorrelationId` | CorrelationIdEnricher | Request correlation ID |
| `MachineName` | Built-in | Server/container name |
| `EnvironmentName` | Built-in | Development/Production |
| `ThreadId` | Built-in | Thread executing the log |
| `Application` | Custom | "IAMS.Api" |
| `Version` | Custom | Application version |

### Custom Enrichers

#### TenantEnricher

**Location:** `src/IAMS.Infrastructure/Logging/Enrichers/TenantEnricher.cs`

Adds tenant context from `HttpContext.Items`:
```csharp
{
    "TenantId": 1,
    "TenantIdentifier": "agency1",
    "TenantName": "ABC Insurance Agency"
}
```

#### UserEnricher

**Location:** `src/IAMS.Infrastructure/Logging/Enrichers/UserEnricher.cs`

Adds user information from JWT claims:
```csharp
{
    "UserId": "user123",
    "Username": "john.doe@agency.com",
    "UserRoles": ["AgencyAdmin", "PolicyManager"]
}
```

#### CorrelationIdEnricher

**Location:** `src/IAMS.Infrastructure/Logging/Enrichers/CorrelationIdEnricher.cs`

Adds correlation ID for request tracing:
```csharp
{
    "CorrelationId": "a1b2c3d4e5f6789"
}
```

---

## Best Practices

### 1. Use Structured Logging

**Always use named parameters:**

```csharp
// âœ… Good - Structured
_logger.LogInformation("Policy {PolicyId} created for customer {CustomerId} with premium {Premium}",
    policy.Id, customer.Id, policy.Premium);

// âŒ Bad - String interpolation
_logger.LogInformation($"Policy {policy.Id} created for customer {customer.Id} with premium {policy.Premium}");
```

### 2. Log at Appropriate Levels

```csharp
// Debug - Detailed diagnostic info
_logger.LogDebug("Calculating premium with base rate {BaseRate} and factors {@Factors}", baseRate, factors);

// Information - Normal flow
_logger.LogInformation("Policy {PolicyId} activated successfully", policyId);

// Warning - Unexpected but recoverable
_logger.LogWarning("Payment {PaymentId} is overdue by {Days} days", paymentId, daysOverdue);

// Error - Failures and exceptions
_logger.LogError(ex, "Failed to process claim {ClaimId}", claimId);

// Critical - System failures
_logger.LogCritical("Database connection pool exhausted!");
```

### 3. Include Relevant Context

```csharp
// âœ… Good - Includes all relevant IDs
_logger.LogInformation(
    "Premium calculated for policy {PolicyId}, customer {CustomerId}, vehicle {VehicleId}: {Premium}",
    policyId, customerId, vehicleId, calculatedPremium);

// âŒ Bad - Missing context
_logger.LogInformation("Premium calculated: {Premium}", calculatedPremium);
```

### 4. Log Exceptions Properly

```csharp
// âœ… Good - Exception as first parameter
try
{
    await ProcessClaim(claim);
}
catch (ClaimProcessingException ex)
{
    _logger.LogError(ex, "Failed to process claim {ClaimId} for policy {PolicyId}",
        claim.Id, claim.PolicyId);
    throw;
}

// âŒ Bad - Exception in message
catch (ClaimProcessingException ex)
{
    _logger.LogError("Failed to process claim: {Message}", ex.Message);
    throw;
}
```

### 5. Avoid Logging Sensitive Data

```csharp
// âœ… Good - No sensitive data
_logger.LogInformation("Payment processed for policy {PolicyId}, amount {Amount}",
    policyId, amount);

// âŒ Bad - Logs credit card number
_logger.LogInformation("Payment processed with card {CardNumber}", payment.CardNumber);

// âœ… Good - Masked sensitive data
_logger.LogInformation("Payment processed with card ending in {LastFourDigits}",
    payment.CardNumber.Substring(payment.CardNumber.Length - 4));
```

### 6. Use Scoped Context for Related Operations

```csharp
public async Task ProcessMonthlyBillingAsync()
{
    using (LogContext.PushProperty("BillingMonth", DateTime.Now.ToString("yyyy-MM")))
    {
        _logger.LogInformation("Starting monthly billing process");

        var policies = await GetActivePolicies();

        foreach (var policy in policies)
        {
            using (LogContext.PushProperty("PolicyId", policy.Id))
            {
                await GenerateInvoice(policy);
            }
        }

        _logger.LogInformation("Monthly billing completed. {PolicyCount} policies processed", policies.Count);
    }
}
```

### 7. Audit Critical Operations

```csharp
public async Task<bool> ApproveClaimAsync(int claimId, decimal approvedAmount)
{
    var claim = await _repository.GetClaimAsync(claimId);

    claim.Status = ClaimStatus.Approved;
    claim.ApprovedAmount = approvedAmount;
    await _repository.UpdateAsync(claim);

    // Audit the approval
    await _auditLogger.LogActionAsync(
        action: "APPROVE",
        entityType: "Claim",
        entityId: claimId.ToString(),
        oldValues: new { Status = ClaimStatus.Pending, ApprovedAmount = 0 },
        newValues: new { Status = ClaimStatus.Approved, ApprovedAmount = approvedAmount },
        description: $"Claim approved for amount {approvedAmount}"
    );

    _logger.LogInformation("Claim {ClaimId} approved for amount {Amount}", claimId, approvedAmount);
    return true;
}
```

### 8. Don't Over-Log

```csharp
// âŒ Bad - Logging inside tight loops
foreach (var item in largeCollection)
{
    _logger.LogDebug("Processing item {ItemId}", item.Id); // Could generate thousands of logs
    ProcessItem(item);
}

// âœ… Good - Log summary
_logger.LogInformation("Processing {ItemCount} items", largeCollection.Count);
foreach (var item in largeCollection)
{
    ProcessItem(item);
}
_logger.LogInformation("Completed processing {ItemCount} items", largeCollection.Count);
```

---

## Troubleshooting

### Common Issues

#### 1. Logs Not Appearing

**Check log level configuration:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"  // Ensure this allows your log level
    }
  }
}
```

**Check environment-specific overrides** in `appsettings.Production.json`.

#### 2. Missing Tenant/User Information

**Verify middleware order:**
```csharp
// Tenant middleware must run before your controllers
app.UseMultiTenancy();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Check that tenant is being set:**
```csharp
var tenantId = HttpContext.Items["TenantId"];
if (tenantId == null)
{
    // Tenant middleware didn't run or failed
}
```

#### 3. Correlation IDs Not Showing

**Ensure CorrelationIdMiddleware is registered:**
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

**Check it runs before your application code:**
The correlation ID middleware should be early in the pipeline.

#### 4. Logs Not Enriched

**Verify enrichers are configured in appsettings.json:**
```json
{
  "Serilog": {
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithThreadId"
    ]
  }
}
```

#### 5. Performance Issues

**Use async sinks for production:**
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Async",
        "Args": {
          "configure": [
            { "Name": "File", "Args": { "path": "logs/app.txt" } }
          ]
        }
      }
    ]
  }
}
```

**Disable HTTP logging in production:**
```json
{
  "Logging": {
    "EnableHttpLogging": false
  }
}
```

### Viewing Logs

#### Console Logs (Development)

```bash
# Run the application
dotnet run --project src/IAMS.Api

# Logs appear in console with color coding
[10:30:15 INF] Starting Insurance Agency Management System API
[10:30:16 INF] HTTP GET /api/policies Request | ContentType: application/json
```

#### File Logs

```bash
# Text logs (human-readable)
tail -f logs/iams-20240115.txt

# JSON logs (structured)
tail -f logs/iams-json-20240115.json | jq .
```

#### Filtering Logs

**By tenant:**
```bash
grep "TenantId.*agency1" logs/iams-20240115.txt
```

**By correlation ID:**
```bash
grep "a1b2c3d4e5f6" logs/iams-20240115.txt
```

**By user:**
```bash
grep "UserId.*user123" logs/iams-20240115.txt
```

**Using jq for JSON logs:**
```bash
# Filter by tenant
cat logs/iams-json-20240115.json | jq 'select(.TenantId == "1")'

# Filter by log level
cat logs/iams-json-20240115.json | jq 'select(.Level == "Error")'

# Filter by time range
cat logs/iams-json-20240115.json | jq 'select(.Timestamp >= "2024-01-15T10:00:00")'
```

---

## Production Deployment

### Pre-Deployment Checklist

- [ ] Configure production log levels (Warning/Error)
- [ ] Disable HTTP logging for performance
- [ ] Set appropriate slow request threshold (5000ms)
- [ ] Configure log retention policy (90 days)
- [ ] Set up log file size limits (100MB)
- [ ] Enable async logging sinks
- [ ] Configure JSON output for log aggregation
- [ ] Test log rotation
- [ ] Set up monitoring and alerts
- [ ] Configure external log aggregation (optional)

### Production Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "IAMS": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "File",
              "Args": {
                "path": "/var/log/iams/iams-json-.json",
                "rollingInterval": "Day",
                "retainedFileCountLimit": 90,
                "fileSizeLimitBytes": 104857600,
                "rollOnFileSizeLimit": true,
                "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
              }
            }
          ]
        }
      }
    ]
  },
  "Logging": {
    "SlowRequestThresholdMs": 5000,
    "EnableHttpLogging": false,
    "EnablePerformanceLogging": true
  }
}
```

### Log Aggregation Integration

#### Elasticsearch/Kibana (ELK Stack)

Use Filebeat to ship JSON logs to Elasticsearch:

```yaml
# filebeat.yml
filebeat.inputs:
  - type: log
    enabled: true
    paths:
      - /var/log/iams/iams-json-*.json
    json.keys_under_root: true
    json.add_error_key: true

output.elasticsearch:
  hosts: ["elasticsearch:9200"]
  index: "iams-logs-%{+yyyy.MM.dd}"
```

#### Seq

Add Seq sink to Serilog:

```bash
dotnet add package Serilog.Sinks.Seq
```

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq-server:5341",
          "apiKey": "your-api-key"
        }
      }
    ]
  }
}
```

#### Azure Application Insights

```bash
dotnet add package Serilog.Sinks.ApplicationInsights
```

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.ApplicationInsights(
        builder.Configuration["ApplicationInsights:InstrumentationKey"],
        TelemetryConverter.Traces)
    .CreateLogger();
```

### Monitoring and Alerts

Set up alerts for:

1. **Error Rate Spike**
   - Trigger: > 10 errors per minute
   - Action: Notify DevOps team

2. **Slow Requests**
   - Trigger: > 5 slow requests per minute
   - Action: Investigate performance

3. **Failed Authentication**
   - Trigger: > 5 failed logins for same user in 5 minutes
   - Action: Potential security threat

4. **Tenant Isolation Issues**
   - Trigger: Logs missing TenantId
   - Action: Critical - investigate immediately

5. **Disk Space**
   - Trigger: Log directory > 80% full
   - Action: Archive old logs

### Log Retention Policy

```bash
# Automated cleanup script (cron job)
#!/bin/bash

# Delete logs older than 90 days
find /var/log/iams -name "iams-*.txt" -mtime +90 -delete
find /var/log/iams -name "iams-json-*.json" -mtime +90 -delete

# Compress logs older than 7 days
find /var/log/iams -name "iams-*.txt" -mtime +7 -exec gzip {} \;
find /var/log/iams -name "iams-json-*.json" -mtime +7 -exec gzip {} \;
```

Add to cron:
```bash
# Run daily at 2 AM
0 2 * * * /usr/local/bin/cleanup-logs.sh
```

---

## Summary

The IAMS logging system provides comprehensive observability for a multi-tenant insurance management system. Key takeaways:

âœ… **Multi-tenant aware** - Every log includes tenant context
âœ… **Request tracing** - Correlation IDs track requests end-to-end
âœ… **Performance monitoring** - Automatic slow request detection
âœ… **Audit compliance** - Comprehensive audit logging for sensitive operations
âœ… **Production ready** - Structured JSON logs, async sinks, configurable retention
âœ… **Developer friendly** - Clear logs in development, detailed errors

For questions or issues, refer to the [troubleshooting section](#troubleshooting) or contact the development team.



---


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



---


# Logging Quick Reference Guide

## Quick Start

### Basic Logging
```csharp
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    public IActionResult Get()
    {
        _logger.LogInformation("Getting data for {Entity}", "Customer");
        return Ok();
    }
}
```

### Audit Logging
```csharp
public class MyService
{
    private readonly IAuditLogger _auditLogger;

    public MyService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task CreateAsync(Entity entity)
    {
        // ... create entity

        await _auditLogger.LogActionAsync(
            action: "CREATE",
            entityType: "Entity",
            entityId: entity.Id.ToString(),
            newValues: entity,
            description: "Created new entity"
        );
    }
}
```

---

## Log Levels Cheat Sheet

| Level | When to Use | Example |
|-------|-------------|---------|
| **Debug** | Detailed diagnostic info | `_logger.LogDebug("Processing with params {@Params}", params);` |
| **Information** | Normal flow, milestones | `_logger.LogInformation("Policy {Id} created", id);` |
| **Warning** | Unexpected but recoverable | `_logger.LogWarning("Payment overdue for {Id}", id);` |
| **Error** | Failures, exceptions | `_logger.LogError(ex, "Failed to process {Id}", id);` |
| **Critical** | System failures | `_logger.LogCritical("Database connection lost");` |

---

## Common Patterns

### Logging with Exception
```csharp
try
{
    await DoWork();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process {Entity} with ID {Id}", "Policy", policyId);
    throw;
}
```

### Structured Logging
```csharp
// âœ… DO THIS
_logger.LogInformation("User {UserId} updated {Entity} {EntityId}",
    userId, "Policy", policyId);

// âŒ DON'T DO THIS
_logger.LogInformation($"User {userId} updated Policy {policyId}");
```

### Scoped Context
```csharp
using Serilog.Context;

using (LogContext.PushProperty("BatchId", batchId))
{
    _logger.LogInformation("Processing batch");
    // All logs here will include BatchId
}
```

### Correlation ID Access
```csharp
var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
```

---

## Audit Logging Methods

### 1. Log Action (CRUD)
```csharp
await _auditLogger.LogActionAsync(
    action: "UPDATE",
    entityType: "Policy",
    entityId: "123",
    oldValues: originalPolicy,
    newValues: updatedPolicy,
    description: "Updated policy premium"
);
```

### 2. Log Authentication
```csharp
await _auditLogger.LogAuthenticationAsync(
    userId: "user123",
    username: "john@example.com",
    success: true,
    reason: "Login successful"
);
```

### 3. Log Authorization
```csharp
await _auditLogger.LogAuthorizationAsync(
    userId: "user123",
    resource: "Policy:123",
    action: "DELETE",
    granted: false,
    reason: "Insufficient permissions"
);
```

### 4. Log Data Access
```csharp
await _auditLogger.LogDataAccessAsync(
    entityType: "Customer",
    entityId: "456",
    action: "VIEW"
);
```

### 5. Log Configuration Change
```csharp
await _auditLogger.LogConfigurationChangeAsync(
    configKey: "MaxPolicyAmount",
    oldValue: 1000000,
    newValue: 2000000,
    reason: "Policy update"
);
```

---

## Configuration Quick Reference

### appsettings.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "IAMS": "Debug"
      }
    }
  },
  "Logging": {
    "SlowRequestThresholdMs": 3000,
    "EnableHttpLogging": true,
    "EnablePerformanceLogging": true
  }
}
```

### Development vs Production

| Setting | Development | Production |
|---------|-------------|------------|
| Default Level | Debug | Warning |
| IAMS Level | Debug | Information |
| HTTP Logging | Enabled | Disabled |
| Slow Threshold | 2000ms | 5000ms |
| Retention | 7 days | 90 days |

---

## Automatic Context

Every log automatically includes:

| Property | Example | Source |
|----------|---------|--------|
| `TenantId` | "1" | Tenant middleware |
| `TenantIdentifier` | "agency1" | Tenant middleware |
| `TenantName` | "ABC Agency" | Tenant middleware |
| `UserId` | "user123" | JWT claims |
| `Username` | "john@example.com" | JWT claims |
| `CorrelationId` | "a1b2c3d4..." | Correlation middleware |
| `MachineName` | "web-server-01" | Environment |
| `EnvironmentName` | "Production" | Environment |

---

## Troubleshooting

### Logs Not Appearing
1. Check log level in `appsettings.json`
2. Verify environment-specific config (`appsettings.Production.json`)
3. Check console output for errors

### Missing Tenant/User Info
1. Ensure middleware order:
   ```csharp
   app.UseMultiTenancy();      // First
   app.UseAuthentication();     // Second
   app.UseAuthorization();      // Third
   ```

### No Correlation IDs
1. Verify `CorrelationIdMiddleware` is registered
2. Check middleware runs before your code

---

## Viewing Logs

### Console (Development)
```bash
dotnet run --project src/IAMS.Api
```

### File Logs
```bash
# Text logs
tail -f logs/iams-20240115.txt

# JSON logs
tail -f logs/iams-json-20240115.json | jq .
```

### Filter by Tenant
```bash
grep "TenantId.*agency1" logs/iams-20240115.txt
```

### Filter by User
```bash
grep "UserId.*user123" logs/iams-20240115.txt
```

### Filter by Correlation ID
```bash
grep "a1b2c3d4e5f6" logs/iams-20240115.txt
```

### Using jq for JSON
```bash
# Errors only
cat logs/iams-json-20240115.json | jq 'select(.Level == "Error")'

# Specific tenant
cat logs/iams-json-20240115.json | jq 'select(.TenantId == "1")'

# Time range
cat logs/iams-json-20240115.json | jq 'select(.Timestamp >= "2024-01-15T10:00:00")'
```

---

## Best Practices

### âœ… DO

- Use structured logging with named parameters
- Log at appropriate levels
- Include relevant context (IDs, entities)
- Audit sensitive operations
- Log exceptions with the exception as first parameter
- Use scoped context for related operations

### âŒ DON'T

- Use string interpolation for log messages
- Log sensitive data (passwords, credit cards, SSN)
- Log inside tight loops
- Swallow exceptions without logging
- Mix logging and string concatenation
- Over-log in production

---

## Example: Complete Service with Logging

```csharp
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;
using Serilog.Context;

public class PolicyService
{
    private readonly ILogger<PolicyService> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly IPolicyRepository _repository;

    public PolicyService(
        ILogger<PolicyService> logger,
        IAuditLogger auditLogger,
        IPolicyRepository repository)
    {
        _logger = logger;
        _auditLogger = auditLogger;
        _repository = repository;
    }

    public async Task<Policy> CreatePolicyAsync(CreatePolicyDto dto)
    {
        using (LogContext.PushProperty("CustomerId", dto.CustomerId))
        using (LogContext.PushProperty("PolicyType", dto.PolicyType))
        {
            _logger.LogInformation("Creating new policy");

            try
            {
                var policy = MapToPolicy(dto);
                await _repository.AddAsync(policy);

                // Audit the creation
                await _auditLogger.LogActionAsync(
                    action: "CREATE",
                    entityType: "Policy",
                    entityId: policy.Id.ToString(),
                    newValues: policy,
                    description: $"Created {policy.PolicyType} policy"
                );

                _logger.LogInformation("Policy {PolicyId} created successfully", policy.Id);
                return policy;
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Policy validation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create policy");
                throw;
            }
        }
    }

    public async Task<Policy> UpdatePolicyAsync(int id, UpdatePolicyDto dto)
    {
        _logger.LogInformation("Updating policy {PolicyId}", id);

        try
        {
            var original = await _repository.GetByIdAsync(id);
            if (original == null)
            {
                _logger.LogWarning("Policy {PolicyId} not found", id);
                throw new NotFoundException($"Policy {id} not found");
            }

            var updated = MapToPolicy(dto);
            updated.Id = id;
            await _repository.UpdateAsync(updated);

            // Audit the update
            await _auditLogger.LogActionAsync(
                action: "UPDATE",
                entityType: "Policy",
                entityId: id.ToString(),
                oldValues: original,
                newValues: updated,
                description: "Updated policy details"
            );

            _logger.LogInformation("Policy {PolicyId} updated successfully", id);
            return updated;
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Failed to update policy {PolicyId}", id);
            throw;
        }
    }

    public async Task DeletePolicyAsync(int id)
    {
        _logger.LogInformation("Deleting policy {PolicyId}", id);

        try
        {
            var policy = await _repository.GetByIdAsync(id);
            if (policy == null)
            {
                _logger.LogWarning("Policy {PolicyId} not found for deletion", id);
                throw new NotFoundException($"Policy {id} not found");
            }

            // Audit the deletion
            await _auditLogger.LogActionAsync(
                action: "DELETE",
                entityType: "Policy",
                entityId: id.ToString(),
                oldValues: policy,
                description: $"Deleted {policy.PolicyType} policy"
            );

            await _repository.DeleteAsync(id);
            _logger.LogWarning("Policy {PolicyId} deleted", id);
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Failed to delete policy {PolicyId}", id);
            throw;
        }
    }
}
```

---

## Support

For detailed documentation, see [LOGGING.md](./LOGGING.md)

For issues, contact the development team or check the troubleshooting section.



---


