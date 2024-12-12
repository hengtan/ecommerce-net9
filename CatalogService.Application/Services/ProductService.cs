using CatalogService.Application.Interfaces;
using CatalogService.Application.DTOs;
using CatalogService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Services
{
    public class ProductService(IProductRepository repository, ILogger<ProductService> logger)
        : IProductService
    {
        // Repositório de produtos
        // Logger para monitoramento

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            logger.LogInformation("Iniciando consulta para o produto com ID {ProductId}", id);

            var product = await repository.GetByIdAsync(id);

            if (product == null)
            {
                logger.LogWarning("Produto com ID {ProductId} não encontrado.", id);
                return null;
            }

            logger.LogInformation("Produto com ID {ProductId} encontrado com sucesso.", id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            };
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            logger.LogInformation("Iniciando consulta para todos os produtos.");

            var products = await repository.GetAllAsync();

            logger.LogInformation("Consulta para todos os produtos concluída. Total: {Count}", products.Count());

            return products.Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            });
        }
    }
}