using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.Persistence.Extensions;
using IAMS.Identity.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Services;
using IAMS.Web.Extensions;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/iams-web-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

builder.Services.AddAntiforgery();
// Add MudBlazor
builder.Services.AddMudServices();

// Add HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Add your existing services
builder.Services.AddMultiTenancyServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration); // Register ASP.NET Identity services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add HTTP Client for API calls
builder.Services.AddHttpClient<IAuthService, AuthService>();

// Add HttpClient with API base URL for Blazor components
// Configuration is environment-aware:
// - Development: Uses appsettings.Development.json (https://localhost:44390)
// - Production: Uses appsettings.Production.json or environment variable ApiSettings__BaseUrl
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var environment = sp.GetRequiredService<IWebHostEnvironment>();

    // Get API base URL from configuration with environment-specific fallback
    var apiBaseUrl = configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrEmpty(apiBaseUrl))
    {
        // Fallback based on environment
        apiBaseUrl = environment.IsDevelopment()
            ? "https://localhost:44390"
            : throw new InvalidOperationException(
                "ApiSettings:BaseUrl is not configured. Please set it in appsettings.json or as an environment variable (ApiSettings__BaseUrl)");
    }

    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

    // Log the configured API base URL for debugging
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("API Base URL configured: {ApiBaseUrl} (Environment: {Environment})",
        apiBaseUrl, environment.EnvironmentName);

    return httpClient;
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();

// Cookie authentication is now configured by AddIdentityServices
// No need for duplicate configuration here

var app = builder.Build();

// Initialize database (seed roles and permissions)
await app.InitializeDatabaseAsync();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
// Multi-tenancy middleware removed - each tenant uses their own database via connection string
// app.UseMiddleware<IAMS.MultiTenancy.Middleware.TenantMiddleware>();



app.MapRazorPages(); 
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseAntiforgery();
app.Run();