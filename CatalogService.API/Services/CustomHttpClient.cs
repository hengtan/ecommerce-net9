using Serilog.Sinks.Http;

namespace CatalogService.API.Services;

public class CustomHttpClient : IHttpClient
{
    private readonly HttpClient _httpClient;

    public CustomHttpClient()
    {
        _httpClient = new HttpClient
        {
            // BaseAddress aponta para o endpoint do Splunk HEC
            BaseAddress = new Uri("http://localhost:8088/services/collector") // Substitua pelo endereço correto do seu Splunk
        };

        // Configura o cabeçalho de autorização com o token do HEC
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Splunk 56f2a517-34e2-4ed3-a97a-15e98f82a22d"); // Substitua pelo seu token HEC
    }

    /// <summary>
    /// Método opcional para configurar o HttpClient usando IConfiguration.
    /// Pode ser usado para carregar configurações adicionais se necessário.
    /// </summary>
    public void Configure(IConfiguration configuration)
    {
        // Implementação opcional caso precise usar IConfiguration
        // Pode ser deixado em branco se não necessário
    }

    /// <summary>
    /// Envia uma solicitação HTTP POST ao Splunk.
    /// </summary>
    public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream, CancellationToken cancellationToken)
    {
        // Prepara o conteúdo da requisição a partir do Stream
        using var content = new StreamContent(contentStream);

        // Envia a requisição POST para o endpoint do HEC
        return await _httpClient.PostAsync(requestUri, content, cancellationToken);
    }

    /// <summary>
    /// Libera os recursos do HttpClient.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

// using Serilog.Sinks.Http;
//
// namespace CatalogService.API.Services;
//
// public class CustomHttpClient : IHttpClient
// {
//     private readonly HttpClient _httpClient;
//
//     public CustomHttpClient()
//     {
//         _httpClient = new HttpClient();
//         _httpClient.DefaultRequestHeaders.Add("Authorization", "Splunk 56f2a517-34e2-4ed3-a97a-15e98f82a22d"); // Token HEC
//     }
//
//     public void Configure(IConfiguration configuration)
//     {
//         // Implementação opcional caso precise usar IConfiguration
//         // Pode ser deixado em branco
//     }
//
//     public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream, CancellationToken cancellationToken)
//     {
//         // Prepara o conteúdo da requisição
//         using var content = new StreamContent(contentStream);
//         return await _httpClient.PostAsync(requestUri, content, cancellationToken);
//     }
//
//     public void Dispose()
//     {
//         // Libera recursos associados ao HttpClient
//         _httpClient.Dispose();
//     }
// }