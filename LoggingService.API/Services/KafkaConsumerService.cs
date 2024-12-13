using Confluent.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"], // Carregar o GroupId
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        if (string.IsNullOrEmpty(config.GroupId))
        {
            throw new ArgumentException("'group.id' é obrigatório e não foi especificado.");
        }

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(configuration["Kafka:Topic"]);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Mensagem consumida: {result.Message.Value}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao consumir mensagem: {ex.Message}");
                }
            }
        }, stoppingToken);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

// using Confluent.Kafka;
// using LoggingService.API.Services;
//
// public class KafkaConsumerService : IKafkaConsumerService
// {
//     private readonly LoggingService.API.Services.LoggingService _loggingService;
//     private readonly ILogger<KafkaConsumerService> _logger;
//     private readonly IConfiguration _configuration;
//
//     public KafkaConsumerService(
//         LoggingService.API.Services.LoggingService loggingService,
//         ILogger<KafkaConsumerService> logger,
//         IConfiguration configuration)
//     {
//         _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//     }
//
//     /// <summary>
//     /// Inicia o consumo do Kafka com o nome do serviço sendo passado dinamicamente.
//     /// </summary>
//     /// <param name="serviceName">O nome do serviço que está chamando este consumidor.</param>
//     /// <param name="stoppingToken">Token para cancelar o processo.</param>
//     public async Task StartConsumingAsync(string serviceName, CancellationToken stoppingToken)
//     {
//         if (string.IsNullOrWhiteSpace(serviceName))
//             throw new ArgumentException("O nome do serviço não pode ser nulo ou vazio.", nameof(serviceName));
//
//         var bootstrapServers = _configuration["Kafka:BootstrapServers"];
//         var topic = _configuration["Kafka:Topic"];
//
//         if (string.IsNullOrEmpty(bootstrapServers) || string.IsNullOrEmpty(topic))
//         {
//             _logger.LogError("Configurações do Kafka estão ausentes.");
//             throw new InvalidOperationException("As configurações do Kafka são obrigatórias.");
//         }
//
//         var consumerConfig = new ConsumerConfig()
//         {
//             BootstrapServers = bootstrapServers,
//             GroupId = "KafkaConsumerGroup",
//             AutoOffsetReset = AutoOffsetReset.Earliest
//         };
//
//         using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
//
//         consumer.Subscribe(topic);
//         _logger.LogInformation($"Kafka Consumer iniciado para o serviço '{serviceName}' no tópico '{topic}'.");
//
//         try
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     var result = consumer.Consume(stoppingToken);
//                     _logger.LogInformation($"Mensagem recebida do Kafka: {result.Message.Value}");
//
//                     // Enviar log para o Elasticsearch via LoggingService
//                     await _loggingService.LogToElasticSearchAsync(serviceName, "Info", result.Message.Value);
//                 }
//                 catch (ConsumeException ex)
//                 {
//                     _logger.LogError($"Erro de consumo no Kafka: {ex.Error.Reason}");
//                 }
//             }
//         }
//         catch (OperationCanceledException)
//         {
//             _logger.LogInformation("Consumo Kafka interrompido.");
//         }
//         finally
//         {
//             consumer.Close();
//             _logger.LogInformation("Kafka Consumer encerrado.");
//         }
//     }
//
//     public async Task ConsumeAsync(CancellationToken stoppingToken)
//     {
//         var bootstrapServers = _configuration["Kafka:BootstrapServers"];
//         var topic = _configuration["Kafka:Topic"];
//
//         if (string.IsNullOrEmpty(bootstrapServers) || string.IsNullOrEmpty(topic))
//         {
//             _logger.LogError("As configurações do Kafka não foram encontradas.");
//             throw new InvalidOperationException("Configurações do Kafka ausentes.");
//         }
//
//         var consumerConfig = new ConsumerConfig
//         {
//             BootstrapServers = bootstrapServers,
//             GroupId = "KafkaConsumerGroup",
//             AutoOffsetReset = AutoOffsetReset.Earliest
//         };
//
//         using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
//         consumer.Subscribe(topic);
//
//         try
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     var result = consumer.Consume(stoppingToken);
//                     _logger.LogInformation($"Mensagem consumida do Kafka: {result.Message.Value}");
//
//                     // Aqui você pode processar a mensagem ou delegar a outro serviço
//                     await _loggingService.LogToElasticSearchAsync("YourServiceName", "Info", result.Message.Value);
//                 }
//                 catch (ConsumeException ex)
//                 {
//                     _logger.LogError($"Erro ao consumir mensagem: {ex.Error.Reason}");
//                 }
//             }
//         }
//         catch (OperationCanceledException)
//         {
//             _logger.LogInformation("Consumo Kafka foi cancelado.");
//         }
//         finally
//         {
//             consumer.Close();
//         }
//     }
// }