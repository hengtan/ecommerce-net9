namespace LoggingService.API.Services
{
    public interface IKafkaConsumerService
    {
        Task ConsumeAsync(CancellationToken stoppingToken);
    }
}