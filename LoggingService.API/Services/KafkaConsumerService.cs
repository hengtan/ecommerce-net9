using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace LoggingService.API.Services
{
    public class KafkaConsumerService : BackgroundService, IKafkaConsumerService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly string _topic;
        private readonly string _bootstrapServers;

        public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topic = configuration["Kafka:Topic"] ?? throw new ArgumentNullException("Kafka:Topic");
            _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new ArgumentNullException("Kafka:BootstrapServers");
        }

        public async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "logging-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka Consumer iniciado e ouvindo o tópico.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Log recebido: {result.Message.Value}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro no consumo do Kafka: {ex.Message}");
                }
            }

            consumer.Close();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConsumeAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}