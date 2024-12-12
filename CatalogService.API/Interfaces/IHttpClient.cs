namespace CatalogService.API.Interfaces;

public interface IHttpClient
{
    void Configure(IConfiguration configuration);
    Task<HttpResponseMessage> PostAsync(string url, Stream content, CancellationToken cancellationToken);
}