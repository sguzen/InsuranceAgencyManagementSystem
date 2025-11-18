# Roles and Permissions Guide

This guide explains how to use the **RoleNames** and **PermissionNames** constants throughout the Insurance Agency Management System.

## Overview

The system uses a **role-based access control (RBAC)** system with fine-grained **permission-based authorization**.

### Key Concepts

- **Roles**: Groups of users with similar access levels (e.g., SuperAdmin, Manager, Agent)
- **Permissions**: Specific actions users can perform (e.g., `customers.create`, `policies.edit`)
- **Claims**: Permissions are stored as claims on user principals and role principals

## Available Roles

Defined in `IAMS.Application.Constants.RoleNames`:

| Role | Description | Use Case |
|------|-------------|----------|
| `SuperAdmin` | Full system access | System administrators, developers |
| `TenantAdmin` | Full tenant access | Tenant administrators |
| `Manager` | Operational management | Branch/office managers |
| `Agent` | Sales and policy management | Insurance agents |
| `AccountingClerk` | Financial operations | Accounting staff |
| `ReportsViewer` | Read-only reporting access | Management, auditors |
| `Viewer` | Read-only access | Customers, limited users |

## Available Permissions

Defined in `IAMS.Application.Constants.PermissionNames`:

### Customer Permissions
- `customers.view` - View customers
- `customers.create` - Create customers
- `customers.edit` - Edit customers
- `customers.delete` - Delete customers

### Policy Permissions
- `policies.view` - View policies
- `policies.create` - Create policies
- `policies.edit` - Edit policies
- `policies.delete` - Delete policies
- `policies.renew` - Renew policies

### Insurance Company Permissions
- `companies.view` - View insurance companies
- `companies.create` - Create insurance companies
- `companies.edit` - Edit insurance companies
- `companies.delete` - Delete insurance companies

### Payment Permissions
- `payments.view` - View payments
- `payments.create` - Create payments
- `payments.edit` - Edit payments
- `payments.delete` - Delete payments

### Claim Permissions
- `claims.view` - View claims
- `claims.create` - Create claims
- `claims.edit` - Edit claims
- `claims.delete` - Delete claims

### Reporting Permissions
- `reports.view` - View reports
- `reports.create` - Create reports
- `reports.export` - Export reports

### Accounting Permissions
- `accounting.view` - View accounting data
- `accounting.commissions` - Manage commissions
- `accounting.financials` - View financial reports

### Integration Permissions
- `integrations.view` - View integrations
- `integrations.manage` - Manage integrations
- `integrations.sync` - Sync data

### Admin Permissions
- `admin.users` - Manage users
- `admin.roles` - Manage roles
- `admin.permissions` - Manage permissions
- `admin.settings` - Manage settings
- `admin.tenant` - Manage tenant
- `admin.modules` - Manage modules

## Usage Examples

### 1. Role-Based Authorization in Razor Pages/Components

```razor
@using IAMS.Application.Constants
@attribute [Authorize(Roles = RoleNames.SuperAdmin)]

<h1>SuperAdmin Only Page</h1>
```

Multiple roles:
```razor
@attribute [Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.TenantAdmin}")]
```

### 2. Permission-Based Authorization in Blazor Components

```razor
@using IAMS.Application.Constants
@inject IPermissionChecker PermissionChecker

@if (canEditCustomers)
{
    <MudButton OnClick="EditCustomer">Edit</MudButton>
}

@code {
    private bool canEditCustomers;

    protected override async Task OnInitializedAsync()
    {
        canEditCustomers = await PermissionChecker.HasPermissionAsync(PermissionNames.EditCustomers);
    }
}
```

### 3. Multiple Permissions (Any/All)

```razor
@code {
    private bool canManageData;
    private bool canDoEverything;

    protected override async Task OnInitializedAsync()
    {
        // User has ANY of these permissions
        canManageData = await PermissionChecker.HasAnyPermissionAsync(
            PermissionNames.CreateCustomers,
            PermissionNames.EditCustomers,
            PermissionNames.DeleteCustomers
        );

        // User has ALL of these permissions
        canDoEverything = await PermissionChecker.HasAllPermissionsAsync(
            PermissionNames.ManageUsers,
            PermissionNames.ManageRoles,
            PermissionNames.ManageSettings
        );
    }
}
```

### 4. Conditional UI Rendering

```razor
@inject IPermissionChecker PermissionChecker

<MudTable Items="@customers">
    <HeaderContent>
        <MudTh>Name</MudTh>
        <MudTh>Email</MudTh>
        @if (canEdit || canDelete)
        {
            <MudTh>Actions</MudTh>
        }
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Name</MudTd>
        <MudTd>@context.Email</MudTd>
        @if (canEdit || canDelete)
        {
            <MudTd>
                @if (canEdit)
                {
                    <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="@(() => Edit(context))" />
                }
                @if (canDelete)
                {
                    <MudIconButton Icon="@Icons.Material.Filled.Delete" OnClick="@(() => Delete(context))" />
                }
            </MudTd>
        }
    </RowTemplate>
</MudTable>

@code {
    private bool canEdit;
    private bool canDelete;

    protected override async Task OnInitializedAsync()
    {
        canEdit = await PermissionChecker.HasPermissionAsync(PermissionNames.EditCustomers);
        canDelete = await PermissionChecker.HasPermissionAsync(PermissionNames.DeleteCustomers);
    }
}
```

### 5. Code-Based Authorization in Services

```csharp
using IAMS.Application.Constants;
using Microsoft.AspNetCore.Authorization;

public class CustomerService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<Result> DeleteCustomerAsync(int customerId)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // Check permission
        if (!user.HasClaim("permission", PermissionNames.DeleteCustomers))
        {
            return Result.Failure("You don't have permission to delete customers");
        }

        // Proceed with deletion
        // ...
    }
}
```

### 6. Checking User Roles Programmatically

```razor
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private bool isSuperAdmin;
    private bool isManager;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        isSuperAdmin = user.IsInRole(RoleNames.SuperAdmin);
        isManager = user.IsInRole(RoleNames.Manager);
    }
}
```

## Automatic Seeding

Roles and their associated permissions are automatically seeded when the application starts:

1. **Database Initialization**: `Program.cs` calls `InitializeDatabaseAsync()`
2. **Role Creation**: All roles from `RoleNames` are created if they don't exist
3. **Permission Assignment**: Each role receives its predefined set of permissions as claims

## Role-Permission Mapping

| Role | Permissions Count | Key Permissions |
|------|-------------------|-----------------|
| SuperAdmin | All (35+) | Everything |
| TenantAdmin | ~30 | All except super admin functions |
| Manager | ~20 | Operational management, reports |
| Agent | ~12 | Customer/policy management |
| AccountingClerk | ~8 | Financial operations |
| ReportsViewer | ~8 | Read-only + reports |
| Viewer | ~5 | Read-only access |

## Best Practices

### ✅ Do's

1. **Use Constants**: Always use `RoleNames` and `PermissionNames` constants
   ```csharp
   // Good ✓
   [Authorize(Roles = RoleNames.SuperAdmin)]

   // Bad ✗
   [Authorize(Roles = "SuperAdmin")]
   ```

2. **Check Permissions in UI**: Hide UI elements users can't use
   ```razor
   @if (await PermissionChecker.HasPermissionAsync(PermissionNames.CreateCustomers))
   {
       <MudButton>Create Customer</MudButton>
   }
   ```

3. **Check Permissions in Services**: Always validate in backend code too
   ```csharp
   if (!user.HasClaim("permission", PermissionNames.EditCustomers))
       return Result.Failure("Unauthorized");
   ```

4. **Use Appropriate Granularity**: Use permissions for fine-grained control, roles for broad categorization

### ❌ Don'ts

1. **Don't hardcode role/permission strings**
2. **Don't rely only on client-side checks** - always validate server-side
3. **Don't forget to await permission checks**
4. **Don't mix authorization approaches** - be consistent

## Adding New Permissions

1. **Add to PermissionNames.cs**:
   ```csharp
   public const string ManageDocuments = "documents.manage";
   ```

2. **Update Role Mappings** in `IdentityDataSeeder.cs`:
   ```csharp
   {
       RoleNames.Manager,
       new List<string>
       {
           // ... existing permissions
           PermissionNames.ManageDocuments
       }
   }
   ```

3. **Restart Application**: Seeds will update automatically

## Troubleshooting

### Users Don't Have Permissions After Login

**Solution**: The database seeder runs on startup. Restart your application after making changes.

### Permission Checks Always Return False

**Check**:
1. User is authenticated
2. Permission constant name matches exactly
3. User's role has the permission assigned
4. Claims are being loaded (check `HttpContext.User.Claims`)

### New Roles Not Appearing

**Solution**: Delete the role from database and restart the application to re-seed.

## Summary

- ✅ Roles defined in `RoleNames.cs`
- ✅ Permissions defined in `PermissionNames.cs`
- ✅ Auto-seeded on application startup
- ✅ Use `IPermissionChecker` in Blazor components
- ✅ Use `[Authorize]` attribute for pages
- ✅ Always use constants, never hardcoded strings
