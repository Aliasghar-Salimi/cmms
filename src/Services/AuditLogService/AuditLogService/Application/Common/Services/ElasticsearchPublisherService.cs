using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using AuditLogService.Domain.Entities;

namespace AuditLogService.Application.Common.Services;

public interface IElasticsearchPublisherService
{
    Task PublishToElasticsearchAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public class ElasticsearchPublisherService : IElasticsearchPublisherService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<ElasticsearchPublisherService> _logger;
    private readonly string _topic;

    public ElasticsearchPublisherService(
        IConfiguration configuration,
        ILogger<ElasticsearchPublisherService> logger)
    {
        _logger = logger;
        _topic = configuration["Kafka:ElasticsearchTopic"] ?? "cmms-audit-logs";

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "audit-log-elasticsearch-publisher",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishToElasticsearchAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        try
        {
            var logData = new
            {
                id = auditLog.Id.ToString(),
                timestamp = auditLog.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                user_id = auditLog.UserId.ToString(),
                user_name = auditLog.UserName,
                action = auditLog.Action,
                entity_name = auditLog.EntityName,
                entity_id = auditLog.EntityId?.ToString(),
                ip_address = auditLog.IpAddress,
                data_before = auditLog.DataBefore,
                data_after = auditLog.DataAfter,
                correlation_id = auditLog.CorrelationId,
                meta_data = auditLog.MetaData,
                service = "cmms",
                log_level = GetLogLevel(auditLog.Action)
            };

            var jsonMessage = JsonSerializer.Serialize(logData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var message = new Message<string, string>
            {
                Key = auditLog.Id.ToString(),
                Value = jsonMessage
            };

            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);
            
            _logger.LogDebug("Published audit log to Elasticsearch topic: {Topic}, Partition: {Partition}, Offset: {Offset}", 
                result.Topic, result.Partition, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit log to Elasticsearch: {AuditLogId}", auditLog.Id);
            throw;
        }
    }

    private static string GetLogLevel(string action)
    {
        return action switch
        {
            var a when a.Contains("Access Denied", StringComparison.OrdinalIgnoreCase) || 
                       a.Contains("Error", StringComparison.OrdinalIgnoreCase) => "WARN",
            _ => "INFO"
        };
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
} 