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
// ✅ DO THIS
_logger.LogInformation("User {UserId} updated {Entity} {EntityId}",
    userId, "Policy", policyId);

// ❌ DON'T DO THIS
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

### ✅ DO

- Use structured logging with named parameters
- Log at appropriate levels
- Include relevant context (IDs, entities)
- Audit sensitive operations
- Log exceptions with the exception as first parameter
- Use scoped context for related operations

### ❌ DON'T

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
