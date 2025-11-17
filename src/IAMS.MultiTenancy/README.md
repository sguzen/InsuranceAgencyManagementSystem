# IAMS.MultiTenancy

## Purpose

The **MultiTenancy** project is the foundational layer that enables database-per-tenant architecture in the Insurance Agency Management System. It provides tenant resolution, metadata management, and dynamic connection routing.

## Why This Layer Is Essential

In a database-per-tenant architecture, each insurance agency gets its own isolated database. However, this creates a critical "chicken-and-egg" problem:

- A request comes in from `agency1.example.com`
- You need to connect to Agency1's database
- But **where is Agency1's connection string stored?**
- It can't be in Agency1's database (you need the connection string to connect to it!)
- **Solution**: Store it in a centralized master database

The MultiTenancy layer solves this by maintaining a **master database** that stores metadata for all tenants, including their database connection strings.

## Architecture Overview

### Two-Database Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     MASTER DATABASE                          │
│                   (TenantDbContext)                          │
├─────────────────────────────────────────────────────────────┤
│  Tables:                                                     │
│  - Tenants        → Tenant metadata & connection strings    │
│  - TenantModules  → Feature flags per tenant                │
└─────────────────────────────────────────────────────────────┘
                            ↓
                   Provides connection info
                            ↓
┌──────────────────┬──────────────────┬──────────────────────┐
│   TENANT DB 1    │   TENANT DB 2    │   TENANT DB 3        │
│ (Agency1 DB)     │ (Agency2 DB)     │ (Agency3 DB)         │
├──────────────────┼──────────────────┼──────────────────────┤
│ - Customers      │ - Customers      │ - Customers          │
│ - Policies       │ - Policies       │ - Policies           │
│ - Users          │ - Users          │ - Users              │
│ - TenantSettings │ - TenantSettings │ - TenantSettings     │
│ (app data)       │ (app data)       │ (app data)           │
└──────────────────┴──────────────────┴──────────────────────┘
```

### Master Database (TenantDbContext)

Stores **tenant metadata** only:

- **Tenants table**: Core tenant information
  - Identifier (subdomain/code)
  - Connection string to tenant's database
  - Subscription info, limits, regional settings

- **TenantModules table**: Which modules are enabled per tenant
  - Policy, Customer, Reporting, Accounting, Integration

### Tenant Databases (ApplicationDbContext)

Each tenant gets their own database with:

- **Business data**: Customers, Policies, Claims, Invoices, etc.
- **Users and permissions**: Identity data (per-tenant users)
- **TenantSettings**: Application settings (stored as JSON)

## Request Flow

```
1. Request arrives → agency1.example.com/policies

2. TenantMiddleware extracts identifier
   - From subdomain: "agency1"
   - OR from header: X-Tenant-ID
   - OR from query param: ?tenant=agency1

3. Query master DB → Get tenant metadata
   SELECT * FROM Tenants WHERE Identifier = 'agency1'
   → Returns: { ConnectionString: "...Database=Agency1DB..." }

4. Set TenantContext
   - Store tenant info in request context
   - Make available via ITenantContextAccessor

5. ApplicationDbContext resolves connection
   - Reads connection string from TenantContext
   - Connects to Agency1's database

6. Request processes with Agency1's data
```

## Key Components

### TenantMiddleware

- Runs early in the ASP.NET Core pipeline
- Extracts tenant identifier from request
- Queries master database for tenant metadata
- Sets up `TenantContext` for the request
- Ensures tenant database exists and is migrated
- Validates tenant is active

### ITenantContextAccessor

- Provides thread-safe access to current tenant information
- Uses `AsyncLocal<T>` for background tasks
- Uses `HttpContext.Items` for web requests
- Exposes: TenantId, ConnectionString, EnabledModules

### ITenantService (MultiTenancy)

**Purpose**: Manage tenant metadata in MASTER database

Methods:
- `GetTenantAsync(identifier)` - Resolve tenant by identifier
- `GetTenantByIdAsync()` - Get tenant by ID
- `UpdateTenantModuleAsync()` - Enable/disable modules
- `InvalidateTenantCacheAsync()` - Clear cached tenant data

**Note**: This is different from `IAMS.Application.Interfaces.ITenantService` which manages settings in each tenant's database.

### TenantDbContext vs ApplicationDbContext

| TenantDbContext | ApplicationDbContext |
|-----------------|----------------------|
| Master database | Tenant-specific database |
| Shared by all tenants | One per tenant |
| Tenant metadata | Business data |
| Connection strings | Actual customer/policy data |
| Module flags | TenantSettings (JSON) |

## Module Management

Modules are features that can be enabled/disabled per tenant:

- **Policy Management**
- **Customer Management**
- **Reporting**
- **Accounting**
- **Integration**

Stored in: `TenantModules` table (master database)

Usage:
```csharp
if (!_tenantService.IsModuleEnabledForCurrentTenant("Reporting"))
{
    // Block access to reporting features
}
```

## Settings Architecture (IMPORTANT)

**Previous approach** (DEPRECATED):
- Settings stored in `Tenant.Settings` JSON column in master DB
- Violated data isolation principle

**Current approach** (since commit 01ab158):
- Settings stored in `TenantSettings` table in **each tenant's database**
- Managed by `IAMS.Infrastructure.Services.ApplicationTenantService`
- Provides true data isolation
- Settings backed up/restored with tenant's business data

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "MasterConnection": "Server=...;Database=IAMS_Master;...",
    "DefaultConnection": "Server=...;Database=IAMS_Default;...",
    "TenantConnections": {
      "agency1": "Server=...;Database=IAMS_Agency1;...",
      "agency2": "Server=...;Database=IAMS_Agency2;..."
    }
  }
}
```

### Service Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddMultiTenancy(builder.Configuration);

// Registers:
// - ITenantContextAccessor
// - ITenantService (MultiTenancy)
// - ICurrentTenantService
// - TenantMiddleware
```

### Middleware

```csharp
app.UseTenantMiddleware(); // Must be early in pipeline
```

## Testing

For integration tests, you can set a specific tenant context:

```csharp
await _tenantContextAccessor.ExecuteWithTenantAsync(tenant, async () =>
{
    // Code runs with specific tenant context
});
```

## Performance Considerations

- Tenant metadata is **cached in memory** (30 minutes)
- Connection string lookups avoid database hits
- Cache invalidation when tenant metadata changes
- Each tenant database is isolated (no cross-tenant queries)

## Migration Strategy

When adding a new tenant:

1. Create entry in `Tenants` table (master DB)
2. Create new database for tenant
3. Run migrations on tenant database
4. Enable desired modules in `TenantModules`
5. Tenant is ready to use

## Common Pitfalls

### ❌ Storing tenant data in master DB
```csharp
// WRONG - Customer data in master DB
masterDb.Customers.Add(customer);
```

### ✅ Store in tenant database
```csharp
// CORRECT - Customer data in tenant's own DB
applicationDb.Customers.Add(customer);
```

### ❌ Hardcoding connection strings
```csharp
// WRONG
var connection = "Server=...;Database=Agency1;...";
```

### ✅ Use TenantContext
```csharp
// CORRECT - Automatically routed to correct tenant DB
var customers = await _context.Customers.ToListAsync();
```

## Future Considerations

- **Database connection pooling** per tenant
- **Tenant provisioning API** for self-service signup
- **Usage metrics** tracking per tenant
- **Automated tenant database backups**
- **Multi-region tenant support**

## Related Documentation

- See `IAMS.Application.Interfaces.ITenantService` for tenant settings management
- See `IAMS.Infrastructure.Services.ApplicationTenantService` for settings implementation
- See `IAMS.Persistence` for ApplicationDbContext setup
