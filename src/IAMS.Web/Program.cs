using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Extensions;
using IAMS.Persistence.Extensions;
using IAMS.Web.Components;
using IAMS.Web.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/iams-web-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// Add your existing services
builder.Services.AddMultiTenancyServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add HTTP Client for API calls
builder.Services.AddHttpClient<IAuthService, AuthService>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/account/logout";
       // options.AccessDeniedPath = "/login"; // Redirect to login instead of access denied
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "IAMS.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax; // Add this to prevent issues
    });

// Add simple authorization
builder.Services.AddAuthorizationCore(options =>
{
   // options.AddPolicy("RequireAuth", policy => policy.RequireAuthenticatedUser());
  //  options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
   // options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
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

// Authentication & Authorization BEFORE tenant middleware
app.UseAuthentication();
app.UseAuthorization();

// Add tenant middleware AFTER auth
app.UseMiddleware<IAMS.MultiTenancy.Middleware.TenantMiddleware>();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();