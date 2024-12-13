using LoggingService.API.Services;
using Serilog;
using LoggingService.API.Clients;

var builder = WebApplication.CreateBuilder(args);

// Configuração do GrpcLogClient
builder.Services.AddSingleton<GrpcLogClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var serverUrl = configuration["Grpc:ServerUrl"]; // Ex: https://localhost:5001
    if (string.IsNullOrEmpty(serverUrl))
        throw new Exception("Grpc:ServerUrl não está configurado no appsettings.json");

    return new GrpcLogClient(serverUrl);
});

// Configurando Serilog para Elasticsearch
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
        new Uri(ctx.Configuration["ElasticSearch:Uri"] ?? throw new Exception("ElasticSearch:Uri não está configurado")))
    {
        IndexFormat = ctx.Configuration["ElasticSearch:IndexFormat"] ?? "default-logs-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true
    }));

// Registrando suporte para HttpClientFactory
builder.Services.AddHttpClient(); // Adiciona o IHttpClientFactory ao container de DI

// Registrando LoggingService no container de DI
builder.Services.AddSingleton<LoggingService.API.Services.LoggingService>();

// Registrando KafkaConsumerService no container de DI
builder.Services.AddHostedService<KafkaConsumerService>();
// builder.Services.AddSingleton<IKafkaConsumerService>(provider =>
// {
//     var loggingService = provider.GetRequiredService<LoggingService.API.Services.LoggingService>();
//     var logger = provider.GetRequiredService<ILogger<KafkaConsumerService>>();
//     var configuration = provider.GetRequiredService<IConfiguration>();
//
//     return new KafkaConsumerService(logger, configuration);
// });

var app = builder.Build();

app.MapGet("/", () => "Logging Service is running!");

app.Run();