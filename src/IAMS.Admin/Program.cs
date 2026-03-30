using IAMS.Admin.Components;
using IAMS.Admin.Services;
using IAMS.Admin.Services.ApiClient;
using IAMS.Identity.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.Persistence.Extensions;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/iams-admin-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AdminAuthenticationStateProvider>();

builder.Services.AddAntiforgery();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();

// Add infrastructure services
builder.Services.AddMultiTenancyServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

// Add HttpClient with API base URL for admin panel
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var environment = sp.GetRequiredService<IWebHostEnvironment>();

    var apiBaseUrl = configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrEmpty(apiBaseUrl))
    {
        apiBaseUrl = environment.IsDevelopment()
            ? "https://localhost:44390"
            : throw new InvalidOperationException(
                "ApiSettings:BaseUrl is not configured. Set it in appsettings.json or as ApiSettings__BaseUrl env var.");
    }

    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

    var apiKey = configuration["ApiSettings:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException(
            "ApiSettings:ApiKey is not configured. Set it in appsettings.json or as ApiSettings__ApiKey env var.");
    }
    httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);

    return httpClient;
});

// Register API Clients
builder.Services.AddScoped<IAgenciesApiClient, AgenciesApiClient>(sp =>
    new AgenciesApiClient(sp.GetRequiredService<HttpClient>()));

builder.Services.AddScoped<IRolesApiClient, RolesApiClient>(sp =>
    new RolesApiClient(sp.GetRequiredService<HttpClient>()));

builder.Services.AddScoped<IUsersApiClient, UsersApiClient>(sp =>
    new UsersApiClient(sp.GetRequiredService<HttpClient>()));

builder.Services.AddScoped<IInsuranceCompaniesApiClient, InsuranceCompaniesApiClient>(sp =>
    new InsuranceCompaniesApiClient(sp.GetRequiredService<HttpClient>()));

builder.Services.AddScoped<IMasterInsuranceCompaniesApiClient, MasterInsuranceCompaniesApiClient>(sp =>
    new MasterInsuranceCompaniesApiClient(sp.GetRequiredService<HttpClient>()));

var app = builder.Build();
var pathBase = app.Configuration["Hosting:PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseAntiforgery();
app.Run();
