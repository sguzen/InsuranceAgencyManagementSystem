using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Logging.Enrichers;
using IAMS.Infrastructure.Services;
using IAMS.Persistence.Extensions;
using IAMS.Identity.Extensions;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Extensions;
using IAMS.Api.Middleware;
using IAMS.Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with custom enrichers
// Note: Machine name, environment name, and thread ID enrichments are configured in appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "IAMS.Api")
    .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add layers
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);
//builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
// Add HTTP Context Accessor (needed for multi-tenancy)
builder.Services.AddHttpContextAccessor();

// Add Multi-Tenancy Services (must be first)
builder.Services.AddMultiTenancyServices(builder.Configuration);

// Add Persistence Services (repositories, Unit of Work, tenant-aware DbContext)
builder.Services.AddPersistenceServices(builder.Configuration);

// Add Application Services (business logic, MediatR, validators)
builder.Services.AddApplicationServices();

// Add Premium Calculation Configuration
builder.Services.AddPremiumCalculationConfiguration(builder.Configuration);

// Add Infrastructure Services (email, file storage, integrations)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register Audit Logger
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

// Add Data Protection for credential encryption
builder.Services.AddDataProtection();

// Register Import Infrastructure Services (ICredentialEncryptionService + IAgencyCredentialService are in AddInfrastructureServices)
builder.Services.AddScoped<IAMS.Persistence.Services.IInsuranceCompanySyncService, IAMS.Persistence.Services.InsuranceCompanySyncService>();
builder.Services.AddScoped<IAMS.Persistence.Services.IPolicyImportService, IAMS.Persistence.Services.PolicyImportService>();

// Register Policy Import Background Service
builder.Services.AddHostedService<IAMS.Persistence.Services.PolicyImportBackgroundService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

// Fail fast on missing/weak secrets — never fall back to a committed default.
// Development: dotnet user-secrets set "JwtSettings:Secret" "<random 32+ chars>"
// Production: environment variable JwtSettings__Secret
if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is not configured or is shorter than 32 characters. " +
        "Set it via user secrets (development) or environment variables (production).");
}

builder.Services.AddAuthentication(options =>
{
    // A policy scheme routes each request to JWT or API key authentication based on the
    // credential the caller presents, so internal service calls (API key) and user calls
    // (JWT) are both authenticated before the tenant middleware inspects the principal.
    options.DefaultScheme = "JwtOrApiKey";
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddPolicyScheme("JwtOrApiKey", "JWT or API Key", options =>
{
    options.ForwardDefaultSelector = context =>
        context.Request.Headers.ContainsKey("X-API-Key")
            ? ApiKeyAuthenticationOptions.DefaultScheme
            : JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddApiKeyAuthentication(options =>
{
    options.ApiKey = builder.Configuration["ApiSettings:ApiKey"] ?? throw new InvalidOperationException("API Key not configured");
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    // Policy that allows either JWT or API Key authentication
    options.AddPolicy("ApiKeyOrJwt", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.DefaultScheme);
        policy.RequireAuthenticatedUser();
    });

    // User/role administration: the internal service (Web enforces its own UI permissions)
    // or a user holding the users/roles management permission.
    options.AddPolicy("UserManagement", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.DefaultScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(IAMS.Shared.Constants.ApplicationConstants.ClaimTypes.ApiKeyValidated, "true") ||
            context.User.HasClaim(IAMS.Shared.Constants.ApplicationConstants.ClaimTypes.Permission, IAMS.Application.Constants.PermissionNames.ManageUsers) ||
            context.User.HasClaim(IAMS.Shared.Constants.ApplicationConstants.ClaimTypes.Permission, IAMS.Application.Constants.PermissionNames.ManageRoles));
    });

    // Secure by default: any endpoint without explicit authorization metadata requires an
    // authenticated caller (JWT or API key). Public endpoints must opt out with [AllowAnonymous].
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.DefaultScheme)
        .RequireAuthenticatedUser()
        .Build();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Insurance Agency Management System API",
        Version = "v1",
        Description = "API for managing insurance agency operations in TRNC"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key for service-to-service authentication. Enter your API key in the text input below.",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

// Add CORS — any origin only in Development; otherwise an explicit allow-list from
// Cors:AllowedOrigins (wildcard subdomains supported, e.g. "https://*.example.com" to
// cover every tenant host under a shared parent domain).
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            if (allowedOrigins.Length == 0)
            {
                Log.Warning("Cors:AllowedOrigins is empty — cross-origin browser requests will be blocked");
            }

            policy.WithOrigins(allowedOrigins)
                  .SetIsOriginAllowedToAllowWildcardSubdomains()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Local development bootstrap (LocalBootstrap=true, set by scripts/local-db.sh): create
// the master, default application, and integration databases from the EF models —
// schema plus HasData seed (default tenant, parametric data) — when they don't exist.
// EnsureCreated is a no-op on databases that already have tables. Never enable this in
// production, where the schema is managed by MasterDbMigrator and DBA scripts.
if (app.Configuration.GetValue<bool>("LocalBootstrap"))
{
    using var bootstrapScope = app.Services.CreateScope();
    bootstrapScope.ServiceProvider.GetRequiredService<IAMS.MultiTenancy.Data.TenantDbContext>()
        .Database.EnsureCreated();
    bootstrapScope.ServiceProvider.GetRequiredService<IAMS.Persistence.Contexts.ApplicationDbContext>()
        .Database.EnsureCreated();
    bootstrapScope.ServiceProvider.GetRequiredService<IAMS.Infrastructure.Data.IntegrationDbContext>()
        .Database.EnsureCreated();
    Log.Information("LocalBootstrap: master, application, and integration databases ensured");
}

// Apply pending master database (TenantDb) schema scripts before serving requests.
// Scripts: src/IAMS.MultiTenancy/Data/Migrations/*.sql, journaled in dbo.__MasterDbMigrations.
// Disable with MasterDb:AutoMigrate=false to run them out-of-band instead.
using (var migrationScope = app.Services.CreateScope())
{
    migrationScope.ServiceProvider.GetRequiredService<IMasterDbMigrator>().Migrate();
}
var pathBase = app.Configuration["Hosting:PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
// IMPORTANT: Middleware order matters!

// 1. Exception handling - must be first to catch all exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Correlation ID - establish request tracing early
app.UseMiddleware<CorrelationIdMiddleware>();

// 3. Performance logging - track full request lifecycle
app.UseMiddleware<PerformanceLoggingMiddleware>();

// 4. HTTP request/response logging (conditionally based on configuration)
var enableHttpLogging = builder.Configuration.GetValue<bool>("Logging:EnableHttpLogging", true);
if (enableHttpLogging)
{
    app.UseMiddleware<HttpLoggingMiddleware>();
}


// 2. Control Swagger via Configuration instead of Environment Variables
var enableSwagger = app.Configuration.GetValue<bool>("EnableSwagger", false);
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Dynamically point to the correct JSON file location
        var swaggerPath = string.IsNullOrWhiteSpace(pathBase) ? "/swagger/v1/swagger.json" : $"{pathBase}/swagger/v1/swagger.json";
        c.SwaggerEndpoint(swaggerPath, "IAMS API v1");
    });
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

// 6. Authentication BEFORE tenant resolution: the tenant middleware trusts the X-Tenant-ID
// header only from API-key-authenticated internal services and validates the JWT's
// tenant_id claim against the resolved tenant.
app.UseAuthentication();

// 7. Multi-tenancy middleware - reads tenant info and connection strings from database
app.UseMultiTenancy();

// 8. Authorization
app.UseAuthorization();

// 8. Application endpoints
app.MapControllers();

try
{
    Log.Information("Starting Insurance Agency Management System API");
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

public partial class Program { }