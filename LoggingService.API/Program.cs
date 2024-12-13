using LoggingService.API.Services;
using Serilog;
using LoggingService.API.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GrpcLogClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new GrpcLogClient(configuration["Grpc:ServerUrl"]); // Ex: https://localhost:5001
});

// Configurando Serilog para Elasticsearch
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
        new Uri(ctx.Configuration["ElasticSearch:Uri"] ?? string.Empty))
    {
        IndexFormat = ctx.Configuration["ElasticSearch:IndexFormat"],
        AutoRegisterTemplate = true
    }));

// Registrando o serviço com sua interface
builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.MapGet("/", () => "Logging Service is running!");

app.Run();