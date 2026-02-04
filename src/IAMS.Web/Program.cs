using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.Persistence.Extensions;
using IAMS.Identity.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Services;
using IAMS.Web.Services.ApiClient;
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
builder.Services.AddControllers();
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
builder.Services.AddWebServices(); // Register Web layer services including AutoMapper profiles

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

    // Add API Key for service-to-service authentication
    var apiKey = configuration["ApiSettings:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException(
            "ApiSettings:ApiKey is not configured. Please set it in appsettings.json or as an environment variable (ApiSettings__ApiKey)");
    }
    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

    // Log the configured API base URL for debugging
    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("API Base URL configured: {ApiBaseUrl} (Environment: {Environment})",
        apiBaseUrl, environment.EnvironmentName);

    return httpClient;
});

// Register API Clients (for calling the shared API)
builder.Services.AddScoped<ICustomersApiClient, CustomersApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new CustomersApiClient(httpClient);
});

builder.Services.AddScoped<ICurrenciesApiClient, CurrenciesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new CurrenciesApiClient(httpClient);
});

builder.Services.AddScoped<IPoliciesApiClient, PoliciesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var policyFormattingService = sp.GetRequiredService<IPolicyFormattingService>();
    return new PoliciesApiClient(httpClient, policyFormattingService);
});

builder.Services.AddScoped<IPaymentsApiClient, PaymentsApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new PaymentsApiClient(httpClient);
});

builder.Services.AddScoped<IInsuranceCompaniesApiClient, InsuranceCompaniesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new InsuranceCompaniesApiClient(httpClient);
});

builder.Services.AddScoped<IPolicyTypesApiClient, PolicyTypesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new PolicyTypesApiClient(httpClient);
});

builder.Services.AddScoped<IVehiclesApiClient, VehiclesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new VehiclesApiClient(httpClient);
});

builder.Services.AddScoped<IParametricApiClient, ParametricApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new ParametricApiClient(httpClient);
});

builder.Services.AddScoped<IUsersApiClient, UsersApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new UsersApiClient(httpClient);
});

builder.Services.AddScoped<IReportingApiClient, ReportingApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new ReportingApiClient(httpClient);
});

builder.Services.AddScoped<IAgenciesApiClient, AgenciesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new AgenciesApiClient(httpClient);
});

builder.Services.AddScoped<IRolesApiClient, RolesApiClient>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    return new RolesApiClient(httpClient);
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



app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseAntiforgery();
app.Run();