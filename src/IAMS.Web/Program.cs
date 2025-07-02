using IAMS.Application.Extensions;
using IAMS.Infrastructure.Data;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Extensions;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Contexts;
using IAMS.Persistence.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/iams-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add HTTP Context Accessor (needed for multi-tenancy)
builder.Services.AddHttpContextAccessor();

// Add Multi-Tenancy Services (must be first)
builder.Services.AddMultiTenancyServices(builder.Configuration);

// Add Persistence Services (repositories, Unit of Work, tenant-aware DbContext)
builder.Services.AddPersistenceServices(builder.Configuration);

// Add Application Services (business logic, MediatR, validators)
builder.Services.AddApplicationServices();

// Add Infrastructure Services (email, file storage, integrations)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Web-specific services (placeholder for future services)
builder.Services.AddWebServices();

// Add authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Add authorization with module-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireReportingModule", policy =>
        policy.RequireAssertion(context =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return false;

            // Get the HTTP context from the resource
            if (context.Resource is HttpContext httpContext)
            {
                var tenantContextAccessor = httpContext.RequestServices
                    .GetRequiredService<ITenantContextAccessor>();
                return tenantContextAccessor.IsModuleEnabled("Reporting");
            }

            return false;
        }));

    options.AddPolicy("RequireAccountingModule", policy =>
        policy.RequireAssertion(context =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return false;

            if (context.Resource is HttpContext httpContext)
            {
                var tenantContextAccessor = httpContext.RequestServices
                    .GetRequiredService<ITenantContextAccessor>();
                return tenantContextAccessor.IsModuleEnabled("Accounting");
            }

            return false;
        }));

    options.AddPolicy("RequireIntegrationModule", policy =>
        policy.RequireAssertion(context =>
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
                return false;

            if (context.Resource is HttpContext httpContext)
            {
                var tenantContextAccessor = httpContext.RequestServices
                    .GetRequiredService<ITenantContextAccessor>();
                return tenantContextAccessor.IsModuleEnabled("Integration");
            }

            return false;
        }));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add tenant resolution middleware (must be early in pipeline)
app.UseMultiTenancy();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ensure all three databases are created
using (var scope = app.Services.CreateScope())
{
    try
    {
        // 1. Master/Tenant Database - stores tenant metadata
        var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        await tenantDbContext.Database.EnsureCreatedAsync();
        Log.Information("Tenant database initialized successfully");

        // 2. Integration Database - stores logs, reports, file metadata
        var integrationDbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
        await integrationDbContext.Database.EnsureCreatedAsync();
        Log.Information("Integration database initialized successfully");

        // 3. Application Database - stores business data (tenant-specific)
        // Note: This uses tenant-aware connection string resolution
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await applicationDbContext.Database.EnsureCreatedAsync();
        Log.Information("Application database initialized successfully");

        Log.Information("All databases initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initializing databases");
        throw; // Re-throw to prevent app from starting with broken database setup
    }
}

app.Run();