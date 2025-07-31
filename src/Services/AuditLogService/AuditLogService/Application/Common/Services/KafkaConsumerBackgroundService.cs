using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuditLogService.Application.Common.Services;

public class KafkaConsumerBackgroundService : BackgroundService
{
    private readonly IKafkaConsumerService _kafkaConsumerService;
    private readonly ILogger<KafkaConsumerBackgroundService> _logger;

    public KafkaConsumerBackgroundService(
        IKafkaConsumerService kafkaConsumerService,
        ILogger<KafkaConsumerBackgroundService> logger)
    {
        _kafkaConsumerService = kafkaConsumerService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kafka consumer background service");

        try
        {
            await _kafkaConsumerService.StartConsumingAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer background service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer background service");
        }
        finally
        {
            await _kafkaConsumerService.StopConsumingAsync();
            _logger.LogInformation("Kafka consumer background service stopped");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Kafka consumer background service");
        await _kafkaConsumerService.StopConsumingAsync();
        await base.StopAsync(cancellationToken);
    }
} 