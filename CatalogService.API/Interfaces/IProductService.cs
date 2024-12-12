using CatalogService.Domain.Entities;

namespace CatalogService.API.Interfaces;

public interface IProductService
{
    Task<Product> GetProductByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
}