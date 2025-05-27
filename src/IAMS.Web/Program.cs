using IAMS.Application.Services.Customers;
using IAMS.Application.Services.Policies;
using IAMS.MultiTenancy.Middleware;
using IAMS.Web.Components;
using IAMS.Web.Components.Account;
using IAMS.Web.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Tenant services
builder.Services.AddMultiTenancy(builder.Configuration);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // Configure identity options
})
.AddEntityFrameworkStores<IdentityDbContext>()
.AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    // Configure JWT options
});

// Application services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
// Add other services

// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add tenant middleware - IMPORTANT: before auth middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
