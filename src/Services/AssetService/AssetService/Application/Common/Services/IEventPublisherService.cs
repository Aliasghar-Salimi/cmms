namespace AssetService.Application.Common.Services;

using AssetService.Application.Common.Events;

public interface IEventPublisherService
{
    Task PublishEventAsync<T>(T @event, string topic) where T : class;
    Task PublishEventAsync<T>(T @event, string topic, string key) where T : class;
    Task PublishEventAsync<T>(T @event, string topic, string key, Dictionary<string, string> headers) where T : class;
    Task PublishBatchAsync<T>(IEnumerable<T> events, string topic) where T : class;
    Task<bool> IsHealthyAsync();
    Task PublishAsync<T>(string eventType, T data) where T : class;
}

public class EventPublisherOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ClientId { get; set; } = "cmms-asset-service";
    public int MessageTimeoutMs { get; set; } = 30000;
    public int RetryBackoffMs { get; set; } = 1000;
    public int MessageSendMaxRetries { get; set; } = 3;
    public bool EnableIdempotence { get; set; } = true;
    public string DefaultTopic { get; set; } = "cmms-asset-events";
} 