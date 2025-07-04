using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Extensions;
using MudBlazor;
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
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

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

// Add Web-specific services
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
if (app.Environment.IsDevelopment())
{
    // Enable detailed developer exception page in development
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

// IMPORTANT: Multi-tenancy middleware must be early in the pipeline
// but AFTER authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add multi-tenancy middleware (must be after authentication/authorization)
app.UseMultiTenancy();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize databases (this might be where the error occurs)
await InitializeDatabasesAsync(app);

app.Run();

// Database initialization method
static async Task InitializeDatabasesAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Initialize master database (this doesn't need tenant context)
        var masterDbContext = serviceProvider.GetRequiredService<IAMS.MultiTenancy.Data.TenantDbContext>();
        await masterDbContext.Database.EnsureCreatedAsync();

        // NOTE: Don't try to initialize tenant databases here during startup
        // They should be initialized when a tenant is first accessed
        // or through a separate initialization process

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Master database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing databases");
        throw;
    }
}