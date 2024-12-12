using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly SqlDbContext _context; // Contexto para o banco SQL
        private readonly IDistributedCache _cache; // Redis para cache
        private readonly ILogger<ProductRepository> _logger; // Logger para monitoramento

        public ProductRepository(SqlDbContext context, IDistributedCache cache, ILogger<ProductRepository> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            var cacheKey = $"product_{id}";

            try
            {
                // Verifica se o produto está no cache Redis
                _logger.LogInformation("Checking Redis cache for product ID: {ProductId}", id);

                var cachedProduct = await _cache.GetStringAsync(cacheKey);
                if (cachedProduct != null)
                {
                    _logger.LogInformation("Product found in Redis cache for ID: {ProductId}", id);
                    return JsonConvert.DeserializeObject<Product>(cachedProduct);
                }
            }
            catch (Exception ex)
            {
                // Loga um erro, mas continua com o fluxo para buscar no banco
                _logger.LogError(ex, "Error accessing Redis cache for product ID: {ProductId}", id);
            }

            // Caso o produto não esteja no Redis, busca no SQL
            _logger.LogWarning("Product not found in Redis cache. Querying SQL database for ID: {ProductId}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogError("Product not found in SQL database for ID: {ProductId}", id);
                return null;
            }

            // Adiciona o produto no cache Redis
            try
            {
                _logger.LogInformation("Adding product to Redis cache for ID: {ProductId}", id);
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(product),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expira em 10 minutos
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to Redis cache for ID: {ProductId}", id);
            }

            return product;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Logs para monitorar a consulta de todos os produtos
            _logger.LogInformation("Consultando todos os produtos no banco de dados.");

            var products = await _context.Products.ToListAsync();

            _logger.LogInformation("Consulta de todos os produtos concluída. Total de produtos encontrados: {Count}", products.Count);

            return products;
        }
    }
}