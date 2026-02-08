using IAMS.Application.Extensions;
using IAMS.Infrastructure.Extensions;
using IAMS.Infrastructure.Interfaces;
using IAMS.Infrastructure.Logging.Enrichers;
using IAMS.Infrastructure.Services;
using IAMS.Persistence.Extensions;
using IAMS.Identity.Extensions;
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

// Register Import Infrastructure Services
builder.Services.AddScoped<IAMS.Infrastructure.Security.ICredentialEncryptionService, IAMS.Infrastructure.Security.CredentialEncryptionService>();
builder.Services.AddScoped<IAMS.Shared.Interfaces.Services.IAgencyCredentialService, IAMS.Infrastructure.Security.AgencyCredentialService>();
builder.Services.AddScoped<IAMS.Persistence.Services.IInsuranceCompanySyncService, IAMS.Persistence.Services.InsuranceCompanySyncService>();
builder.Services.AddScoped<IAMS.Persistence.Services.IPolicyImportService, IAMS.Persistence.Services.PolicyImportService>();

// Register Policy Import Background Service
builder.Services.AddHostedService<IAMS.Persistence.Services.PolicyImportBackgroundService>();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

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

// 5. Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IAMS API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

// 6. Multi-tenancy middleware - disabled (each tenant uses their own database via connection string)
// app.UseMultiTenancy();

// 7. Authentication & Authorization
app.UseAuthentication();
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