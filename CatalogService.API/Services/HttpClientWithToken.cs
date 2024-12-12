using CatalogService.API.Interfaces;

namespace CatalogService.API.Services;

public class HttpClientWithToken(HttpClient httpClient) : IHttpClient
{
    private string _authToken;

    public void Configure(IConfiguration configuration)
    {
        // Configure the client using the provided configuration
        _authToken = configuration["ApiSettings:AuthToken"];
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, Stream content, CancellationToken cancellationToken)
    {
        var streamContent = new StreamContent(content);
        return await httpClient.PostAsync(url, streamContent, cancellationToken);
    }
}