using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Text;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MediatR;
using FluentValidation;
using IdentityService.Application.Features.Users.Validators;
using IdentityService.Application.Features.Users.Commands.CreateUser;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Mapping;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Authorization;
using IdentityService.Application.Common.Services;
using Serilog;
using Serilog.Events;
using Confluent.Kafka;
using Serilog.Enrichers;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var kafkaTopic = builder.Configuration["Kafka:Topic"] ?? "cmms-identity-service-topic";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Async(sink => sink.Sink(new KafkaAuditLogSink(kafkaTopic, kafkaBootstrapServers)))
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing current HTTP context
builder.Services.AddHttpContextAccessor();

// Add API Versioning
builder.Services.AddApiVersioningServices();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-here"))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with versioning
builder.Services.AddSwaggerVersioning();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();

// Add RBAC Authorization
builder.Services.AddRbacAuthorization();

// Add Authentication Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Add Audit Log Service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Add SMS Services
builder.Services.AddScoped<ISmsService, KavenegarSmsService>();
builder.Services.AddScoped<ISmsVerificationService, SmsVerificationService>();

// Add HttpClient for SMS service
builder.Services.AddHttpClient();

// Add DbContext
builder.Services.AddDbContext<IdentityServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity without cookie authentication for API-only usage
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<IdentityServiceDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"CMMS Identity Service API {description.GroupName.ToUpperInvariant()}");
        }
        
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CMMS Identity Service API Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

// app.UseHttpsRedirection(); // Disabled for development

app.UseAuthentication();
app.UseAuthorization();

// Global exception handler for authentication errors
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (UnauthorizedAccessException)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "Authentication failed",
            message = "Invalid or expired token",
            statusCode = 401
        }));
    }
});

app.MapControllers();

// Minimal API endpoints
app.MapGet("/", () => Results.Ok(new { 
    Message = "CMMS Identity Service is running",
    Endpoints = new {
        Health = "/health",
        Version = "/api/v1/version",
        Swagger = "/swagger",
        Api = "/api/v1"
    },
    Timestamp = DateTime.UtcNow,
    Version = VersionInfo.GetVersionInfo()
}));

app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "CMMS Identity Service",
    Version = VersionInfo.InformationalVersion,
    BuildVersion = VersionInfo.Version
}));

// Seed database with initial data
await DatabaseSeeder.SeedAsync(app.Services);

app.Run();
