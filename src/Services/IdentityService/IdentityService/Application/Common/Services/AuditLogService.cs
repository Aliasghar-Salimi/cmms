using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Common.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IConfiguration configuration, ILogger<AuditLogService> logger)
    {
        _logger = logger;
        _topic = configuration["Kafka:AuditTopic"] ?? "cmms-audit-logs";
        
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "cmms-identity-service-audit",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };
        
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task LogLoginAsync(Guid userId, string userName, string email, string ipAddress, bool isSuccess, string? failureReason = null, string? correlationId = null)
    {
        var auditLog = new
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserName = userName,
            Action = isSuccess ? "LOGIN_SUCCESS" : "LOGIN_FAILED",
            EntityName = "User",
            EntityId = userId,
            IpAddress = ipAddress,
            DataBefore = (string?)null,
            DataAfter = isSuccess ? JsonSerializer.Serialize(new { Email = email, LoginTime = DateTime.UtcNow }) : null,
            CorrelationId = correlationId,
            MetaData = JsonSerializer.Serialize(new 
            { 
                Email = email,
                FailureReason = failureReason,
                IsSuccess = isSuccess
            })
        };

        await SendAuditLogAsync(auditLog);
    }

    public async Task LogLogoutAsync(Guid userId, string userName, string ipAddress, string? correlationId = null)
    {
        var auditLog = new
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserName = userName,
            Action = "LOGOUT",
            EntityName = "User",
            EntityId = userId,
            IpAddress = ipAddress,
            DataBefore = (string?)null,
            DataAfter = JsonSerializer.Serialize(new { LogoutTime = DateTime.UtcNow }),
            CorrelationId = correlationId,
            MetaData = JsonSerializer.Serialize(new { LogoutTime = DateTime.UtcNow })
        };

        await SendAuditLogAsync(auditLog);
    }

    public async Task LogActionAsync(Guid userId, string userName, string action, string? entityName = null, Guid? entityId = null, string? ipAddress = null, string? dataBefore = null, string? dataAfter = null, string? correlationId = null, string? metaData = null)
    {
        var auditLog = new
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = ipAddress,
            DataBefore = dataBefore,
            DataAfter = dataAfter,
            CorrelationId = correlationId,
            MetaData = metaData
        };

        await SendAuditLogAsync(auditLog);
    }

    private async Task SendAuditLogAsync(object auditLog)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(auditLog, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = jsonMessage
            };

            var deliveryResult = await _producer.ProduceAsync(_topic, message);
            
            _logger.LogInformation("Audit log sent to Kafka: {Topic} - {Partition} - {Offset}", 
                deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send audit log to Kafka");
            // Don't throw - audit logging should not break the main flow
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
} 