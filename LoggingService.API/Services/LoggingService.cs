using System.Text;
using Newtonsoft.Json;

namespace LoggingService.API.Services;

public class LoggingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoggingService> _logger;
    private readonly string _elasticsearchEndpoint;
    private readonly string _username;
    private readonly string _password;

    public LoggingService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoggingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _elasticsearchEndpoint = configuration["ElasticSearch:Uri"] ?? throw new Exception("ElasticSearch:Uri não configurado.");
        _username = configuration["ElasticSearch:Username"] ?? throw new Exception("ElasticSearch:Username não configurado.");
        _password = configuration["ElasticSearch:Password"] ?? throw new Exception("ElasticSearch:Password não configurado.");
    }

    public async Task LogToElasticSearchAsync(string serviceName, string message, string level = "Info")
    {
        var jsonLog = new
        {
            timestamp = DateTime.UtcNow,
            level,
            message,
            application = serviceName // O nome do serviço que chamou o método
        };

        // Adicione este log para inspecionar o conteúdo do JSON antes de enviar
        _logger.LogInformation($"JSON gerado para Elasticsearch: {JsonConvert.SerializeObject(jsonLog)}");
        
        // Gerar o índice com base na data atual
        var indexName = $"catalog-logs-{DateTime.UtcNow:yyyy.MM.dd}";

        using var client = _httpClientFactory.CreateClient();

        // Adiciona autenticação básica se necessário
        var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var content = new StringContent(JsonConvert.SerializeObject(jsonLog), Encoding.UTF8, "application/json");

        try
        {
            // Substitua o wildcard por um índice específico
            var response = await client.PostAsync($"{_elasticsearchEndpoint}/{indexName}/_doc", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Payload enviado: {JsonConvert.SerializeObject(jsonLog)}");
                _logger.LogInformation("Log enviado ao Elasticsearch com sucesso.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Falha ao enviar log para Elasticsearch: {response.StatusCode} - {errorContent}");
                _logger.LogError($"Payload enviado: {JsonConvert.SerializeObject(jsonLog)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao tentar enviar log para Elasticsearch: {ex.Message}");
        }
    }
}