using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using AssetService.Application.Common.Events;

namespace AssetService.Application.Common.Services;

public class KafkaEventPublisherService : IEventPublisherService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisherService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _defaultTopic;

    public KafkaEventPublisherService(
        EventPublisherOptions options,
        ILogger<KafkaEventPublisherService> logger)
    {
        _logger = logger;
        _defaultTopic = options.DefaultTopic;
        
        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            ClientId = options.ClientId,
            MessageTimeoutMs = options.MessageTimeoutMs,
            RetryBackoffMs = options.RetryBackoffMs,
            MessageSendMaxRetries = options.MessageSendMaxRetries,
            EnableIdempotence = options.EnableIdempotence,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _logger.LogInformation("KafkaEventPublisherService initialized with bootstrap servers: {BootstrapServers}", 
            options.BootstrapServers);
    }

    public async Task PublishEventAsync<T>(T @event, string topic) where T : class
    {
        await PublishEventAsync(@event, topic, Guid.NewGuid().ToString());
    }

    public async Task PublishEventAsync<T>(T @event, string topic, string key) where T : class
    {
        await PublishEventAsync(@event, topic, key, new Dictionary<string, string>());
    }

    public async Task PublishEventAsync<T>(T @event, string topic, string key, Dictionary<string, string> headers) where T : class
    {
        try
        {
            var jsonEvent = JsonSerializer.Serialize(@event, _jsonOptions);
            
            var message = new Message<string, string>
            {
                Key = key,
                Value = jsonEvent
            };
            
            if (headers != null && headers.Any())
            {
                var kafkaHeaders = new Headers();
                foreach (var header in headers)
                {
                    kafkaHeaders.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
                }
                message.Headers = kafkaHeaders;
            }

            var deliveryResult = await _producer.ProduceAsync(topic, message);
            
            _logger.LogInformation("Event published successfully to topic {Topic} with key {Key}. Partition: {Partition}, Offset: {Offset}", 
                topic, key, deliveryResult.Partition, deliveryResult.Offset);

            // Add correlation ID to headers if it's a BaseEvent
            if (@event is BaseEvent baseEvent && !string.IsNullOrEmpty(baseEvent.CorrelationId))
            {
                _logger.LogInformation("Event {EventType} published with correlation ID {CorrelationId}", 
                    baseEvent.EventType, baseEvent.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to topic {Topic} with key {Key}", topic, key);
            throw new InvalidOperationException($"Failed to publish event to Kafka: {ex.Message}", ex);
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, string topic) where T : class
    {
        try
        {
            var tasks = events.Select(async @event =>
            {
                var key = @event is BaseEvent baseEvent ? baseEvent.EventId.ToString() : Guid.NewGuid().ToString();
                await PublishEventAsync(@event, topic, key);
            });

            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Successfully published {EventCount} events to topic {Topic}", 
                events.Count(), topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch of events to topic {Topic}", topic);
            throw new InvalidOperationException($"Failed to publish batch of events to Kafka: {ex.Message}", ex);
        }
    }

    public async Task PublishAsync<T>(string eventType, T data) where T : class
    {
        try
        {
            var jsonEvent = JsonSerializer.Serialize(data, _jsonOptions);
            
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = jsonEvent
            };

            var deliveryResult = await _producer.ProduceAsync(_defaultTopic, message);
            
            _logger.LogInformation("Event {EventType} published successfully to topic {Topic}. Partition: {Partition}, Offset: {Offset}", 
                eventType, _defaultTopic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to topic {Topic}", eventType, _defaultTopic);
            throw new InvalidOperationException($"Failed to publish event to Kafka: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // Create a test message to verify connectivity
            var testMessage = new Message<string, string>
            {
                Key = "health-check",
                Value = "health-check-message"
            };

            var result = await _producer.ProduceAsync(_defaultTopic, testMessage);
            return result.Status == PersistenceStatus.Persisted;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for Kafka producer");
            return false;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
} 