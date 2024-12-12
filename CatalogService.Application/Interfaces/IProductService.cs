using CatalogService.Application.DTOs;

namespace CatalogService.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de aplicação de produtos.
    /// Define métodos para gerenciar produtos.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Obtém os detalhes de um produto pelo ID.
        /// </summary>
        /// <param name="id">ID do produto.</param>
        /// <returns>Um objeto ProductDto representando o produto.</returns>
        Task<ProductDto> GetProductByIdAsync(Guid id);

        /// <summary>
        /// Obtém a lista de todos os produtos disponíveis.
        /// </summary>
        /// <returns>Uma lista de ProductDto representando todos os produtos.</returns>
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    }
}