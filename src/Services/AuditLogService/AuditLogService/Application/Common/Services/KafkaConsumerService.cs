using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AuditLogService.Domain.Entities;
using AuditLogService.Infrastructure.Persistence;

namespace AuditLogService.Application.Common.Services;

public class KafkaConsumerService : IKafkaConsumerService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _topic;
    private bool _isConsuming = false;

    public KafkaConsumerService(
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<KafkaConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _topic = configuration["Kafka:AuditTopic"] ?? "cmms-audit-logs";

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "audit-log-service-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        if (_isConsuming)
        {
            _logger.LogWarning("Consumer is already running");
            return;
        }

        _isConsuming = true;
        _consumer.Subscribe(_topic);

        _logger.LogInformation("Started consuming from topic: {Topic}", _topic);

        try
        {
            while (_isConsuming && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    if (consumeResult?.Message?.Value != null)
                    {
                        await ProcessMessageAsync(consumeResult.Message.Value, cancellationToken);
                        _consumer.Commit(consumeResult);
                        _consumer.StoreOffset(consumeResult);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consuming operation was cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    await Task.Delay(1000, cancellationToken); // Wait before retrying
                }
            }
        }
        finally
        {
            _isConsuming = false;
            _consumer.Close();
        }
    }

    public async Task StopConsumingAsync()
    {
        _isConsuming = false;
        _logger.LogInformation("Stopping Kafka consumer");
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing message: {Message}", message);

            var auditLogData = JsonSerializer.Deserialize<AuditLogData>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (auditLogData == null)
            {
                _logger.LogWarning("Failed to deserialize audit log message");
                return;
            }

            // Create a new scope for each message to properly handle DbContext
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuditLogServiceDbContext>();

            var auditLog = new AuditLog
            {
                Id = auditLogData.Id,
                Timestamp = auditLogData.Timestamp,
                UserId = auditLogData.UserId,
                UserName = auditLogData.UserName,
                Action = auditLogData.Action,
                EntityName = auditLogData.EntityName,
                EntityId = auditLogData.EntityId,
                IpAddress = auditLogData.IpAddress,
                DataBefore = auditLogData.DataBefore,
                DataAfter = auditLogData.DataAfter,
                CorrelationId = auditLogData.CorrelationId,
                MetaData = auditLogData.MetaData
            };

            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit log saved to database: {Action} by {UserName} at {Timestamp}", 
                auditLog.Action, auditLog.UserName, auditLog.Timestamp);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize audit log message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audit log message: {Message}", message);
        }
    }

    public void Dispose()
    {
        _consumer?.Dispose();
    }

    // DTO for deserializing Kafka messages
    private class AuditLogData
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? EntityName { get; set; }
        public Guid? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string? DataBefore { get; set; }
        public string? DataAfter { get; set; }
        public string? CorrelationId { get; set; }
        public string? MetaData { get; set; }
    }
} 