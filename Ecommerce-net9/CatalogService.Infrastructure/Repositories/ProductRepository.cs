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

            // Verifica se o produto está no Redis
            _logger.LogInformation("Verificando cache Redis para o produto com ID {ProductId}", id);

            var cachedProduct = await _cache.GetStringAsync(cacheKey);
            if (cachedProduct != null)
            {
                // Produto encontrado no cache
                _logger.LogInformation("Produto encontrado no Redis. Retornando o cache para o ID {ProductId}", id);
                return JsonConvert.DeserializeObject<Product>(cachedProduct);
            }

            // Produto não encontrado no Redis, consultando o SQL
            _logger.LogWarning("Produto não encontrado no Redis. Consultando banco de dados para o ID {ProductId}", id);

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // Adiciona o produto ao cache Redis
                _logger.LogInformation("Adicionando produto no cache Redis para o ID {ProductId}", id);

                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(product),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache expira em 10 minutos
                    }
                );
            }
            else
            {
                _logger.LogError("Produto com ID {ProductId} não encontrado no banco de dados.", id);
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