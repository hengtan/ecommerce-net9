using CatalogService.API.Interfaces;
using CatalogService.API.Services;
using CatalogService.Infrastructure;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using IProductService = CatalogService.Application.Interfaces.IProductService;
using ProductService = CatalogService.Application.Services.ProductService;

var builder = WebApplication.CreateBuilder(args);

// Lê a configuração do Elasticsearch
var elasticConfig = builder.Configuration.GetSection("ElasticSearch");
var elasticUri = elasticConfig["Uri"] ?? throw new ArgumentNullException(nameof(elasticConfig));
var indexFormat = elasticConfig["IndexFormat"] ?? "default-logs-{0:yyyy.MM.dd}";

Serilog.Debugging.SelfLog.Enable(Console.Error);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true, // Registra automaticamente o template
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv8, // Especifica a versão do Elasticsearch
        IndexFormat = "catalog-logs-{0:yyyy.MM.dd}",
        ModifyConnectionSettings = connection =>
            connection.BasicAuthentication("elastic", "hK=OoK*PkRvyOF2*ov=O"), // Atualize com sua senha correta
        FailureCallback = e => Console.WriteLine($"Falha ao enviar log para Elasticsearch: {e.MessageTemplate}"),
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                           EmitEventFailureHandling.WriteToFailureSink |
                           EmitEventFailureHandling.RaiseCallback
    })
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Substitui o provedor de logs padrão pelo Serilog
builder.Host.UseSerilog();

// Configuração do banco de dados SQL
builder.Services.AddDbContext<SqlDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
    if (string.IsNullOrEmpty(redisConnection))
    {
        throw new InvalidOperationException("Redis connection string is not configured.");
    }

    options.Configuration = redisConnection;
    builder.Logging.AddConsole(); // Adiciona logs no console para monitorar o Redis
    builder.Logging.AddDebug();   // Adiciona logs de debug para Redis
});

// Registro de dependências (Injeção de Dependência)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpClient<IHttpClient, HttpClientWithToken>();

// Configuração de serviços gRPC
builder.Services.AddGrpc();

// Configuração do Swagger para visualização
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Testar envio de log
// Teste inicial de logs
Log.Information("Aplicação iniciada com sucesso!");
Log.Warning("Este é um aviso de teste para Elasticsearch!");
Log.Error("Erro de teste enviado ao Elasticsearch!");


var app = builder.Build();

// Ativa o Swagger no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeia o serviço gRPC
app.MapGrpcService<CatalogService.API.Services.ProductService>();

// Teste inicial de log para garantir que o Serilog está funcionando
Log.Information("Application has started and is logging through Serilog!");

// Adicionando endpoints simples para teste
app.MapGet("/", () => "Hello World!");

app.Run("http://localhost:8000"); // Configurando para rodar na porta 8000

// Fecha e descarrega o Serilog adequadamente no encerramento da aplicação
Log.CloseAndFlush();