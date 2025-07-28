using Serilog.Core;
using Serilog.Events;
using Confluent.Kafka;
using System.Text.Json;

namespace IdentityService.Application.Common.Services;

public class KafkaAuditLogSink : ILogEventSink
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaAuditLogSink(string topic, string bootstrapServers)
    {
        _topic = topic;
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "cmms-identity-service",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            var logMessage = new
            {
                Timestamp = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString(),
                Properties = logEvent.Properties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                )
            };

            var jsonMessage = JsonSerializer.Serialize(logMessage);
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = jsonMessage
            };

            _producer.Produce(_topic, message, deliveryReport =>
            {
                if (deliveryReport.Error.IsError)
                {
                    Console.WriteLine($"Failed to deliver message to Kafka: {deliveryReport.Error.Reason}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending log to Kafka: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
} 