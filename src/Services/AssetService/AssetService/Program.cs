using AssetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AssetService.Application.Mapping;
using AssetService.Application.Features.Asset.Commands.CreateAsset;
using AssetService.Application.Common;
using AssetService.Application.Common.Services;
using AssetService.Application.Common.Saga;
using FluentValidation;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1.0", new() { Title = "Asset Service API v1.0", Version = "v1.0" });
    c.SwaggerDoc("v2.0", new() { Title = "Asset Service API v2.0", Version = "v2.0" });
    
    // Enable annotations
    c.EnableAnnotations();
    
    // Add API versioning support
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Simplified version without TryGetMethodInfo
        return true;
    });
});

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateAssetCommand).Assembly);
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateAssetCommandValidator>();

// Add DbContext
builder.Services.AddDbContext<AssetServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient for Identity Service
builder.Services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>();

// Add Identity Service Client
builder.Services.Configure<IdentityServiceClientOptions>(options =>
{
    options.BaseUrl = builder.Configuration["IdentityService:BaseUrl"] ?? "http://localhost:5226";
    options.TimeoutSeconds = int.Parse(builder.Configuration["IdentityService:TimeoutSeconds"] ?? "30");
    options.MaxRetries = int.Parse(builder.Configuration["IdentityService:MaxRetries"] ?? "3");
    options.RetryDelayMilliseconds = int.Parse(builder.Configuration["IdentityService:RetryDelayMilliseconds"] ?? "1000");
});

// Add Kafka Event Publisher
builder.Services.AddSingleton<EventPublisherOptions>(serviceProvider =>
{
    return new EventPublisherOptions
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:29092",
        DefaultTopic = builder.Configuration["Kafka:DefaultTopic"] ?? "cmms-asset-events",
        ClientId = "cmms-asset-service",
        MessageTimeoutMs = 30000,
        RetryBackoffMs = 1000,
        MessageSendMaxRetries = 3,
        EnableIdempotence = true
    };
});

builder.Services.AddSingleton<IEventPublisherService, KafkaEventPublisherService>();

// Add Saga Orchestrator
builder.Services.AddScoped<ISagaOrchestrator, SagaOrchestrator>();

// Add Saga State Repository
builder.Services.AddScoped<ISagaStateRepository, SagaStateRepository>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Asset Service API v1.0");
        c.SwaggerEndpoint("/swagger/v2.0/swagger.json", "Asset Service API v2.0");
    });
}

// Enable CORS
app.UseCors("AllowAll");

// Use global exception handler
app.UseMiddleware<GlobalExceptionHandler>();

// Use routing and controllers
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new {
    status = "healthy",
    service = "Asset Service",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.Run();