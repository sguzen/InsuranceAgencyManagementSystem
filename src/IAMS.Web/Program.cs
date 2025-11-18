using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.Persistence.Extensions;
using IAMS.Identity.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Services;
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
builder.Services.AddScoped(sp =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:44390";
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    return httpClient;
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();

// Cookie authentication is now configured by AddIdentityServices
// No need for duplicate configuration here

var app = builder.Build();

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