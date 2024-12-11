using CatalogService.Application.Interfaces;
using CatalogService.Application.DTOs;
using CatalogService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository; // Repositório de produtos
        private readonly ILogger<ProductService> _logger; // Logger para monitoramento

        public ProductService(IProductRepository repository, ILogger<ProductService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            _logger.LogInformation("Iniciando consulta para o produto com ID {ProductId}", id);

            var product = await _repository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado.", id);
                return null;
            }

            _logger.LogInformation("Produto com ID {ProductId} encontrado com sucesso.", id);

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
            _logger.LogInformation("Iniciando consulta para todos os produtos.");

            var products = await _repository.GetAllAsync();

            _logger.LogInformation("Consulta para todos os produtos concluída. Total: {Count}", products.Count());

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