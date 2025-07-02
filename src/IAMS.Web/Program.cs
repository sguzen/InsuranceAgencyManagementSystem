using IAMS.Application.Extensions;
using IAMS.Infrastructure.Data;
using IAMS.Infrastructure.Extensions;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Extensions;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Middleware;
using IAMS.Persistence.Contexts;
using IAMS.Web.Components;
using IAMS.Web.Services;
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

// Add Entity Framework DbContexts
// Master Database (for tenant management)
builder.Services.AddDbContext<TenantDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterConnection"));
});

// Application Database (for business data) - will be tenant-specific
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMultiTenancyServices(builder.Configuration);

// Add MudBlazor
builder.Services.AddMudServices();

// Add Blazor-specific services
builder.Services.AddScoped<ICustomerComponentService, CustomerComponentService>();
builder.Services.AddScoped<IPolicyComponentService, PolicyComponentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJSInteropService, JSInteropService>();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireReportingModule", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity.IsAuthenticated &&
            context.HttpContext.RequestServices
                .GetRequiredService<ITenantContextAccessor>()
                .IsModuleEnabled("Reporting")));

    options.AddPolicy("RequireAccountingModule", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity.IsAuthenticated &&
            context.HttpContext.RequestServices
                .GetRequiredService<ITenantContextAccessor>()
                .IsModuleEnabled("Accounting")));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Custom middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
        var appContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure databases are created
        await masterContext.Database.EnsureCreatedAsync();
        await appContext.Database.EnsureCreatedAsync();

        Log.Information("Databases initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initializing databases");
    }
}

try
{
    Log.Information("Starting IAMS Blazor Application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}