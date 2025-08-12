using Confluent.Kafka;
using Serilog;
using Serilog.Events;
using AuditLogService;
using AuditLogService.Infrastructure.Persistence;
using AuditLogService.Application.Common.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add db context
builder.Services.AddDbContext<AuditLogServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Kafka consumer services - Changed to Singleton to fix DI issue
builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
builder.Services.AddSingleton<IElasticsearchPublisherService, ElasticsearchPublisherService>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/audit-log.log", rollingInterval: RollingInterval.Day));

var app = builder.Build();

// Configure Kestrel to bind to all interfaces
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:80");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok(new { 
    Message = "CMMS Audit Log Service is running",
    Endpoints = new {
        Health = "/health",
        AuditLogs = "/api/auditlogs",
        Stats = "/api/auditlogs/stats",
        Swagger = "/swagger"
    },
    Timestamp = DateTime.UtcNow,
    Version = VersionInfo.GetVersionInfo()
}));

app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Service = "CMMS Audit Log Service",
    Version = VersionInfo.InformationalVersion,
    BuildVersion = VersionInfo.Version
}));

app.Run();

