using CatalogService.API.Grpc;
using Grpc.Core;
using CatalogService.Application.Interfaces;

namespace CatalogService.API.Services
{
    /// <summary>
    /// Serviço gRPC para gerenciamento de produtos.
    /// </summary>
    public class ProductService : Grpc.ProductService.ProductServiceBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Construtor com injeção de dependência.
        /// </summary>
        /// <param name="productService">Serviço de aplicação para produtos.</param>
        public ProductService(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Busca os detalhes de um produto pelo ID.
        /// </summary>
        /// <param name="request">Requisição contendo o ID do produto.</param>
        /// <param name="context">Contexto da chamada gRPC.</param>
        /// <returns>Resposta contendo os detalhes do produto.</returns>
        public override async Task<ProductResponse> GetProductById(ProductRequest request, ServerCallContext context)
        {
            // Log de início do método
            Console.WriteLine($"Recebida solicitação para produto ID: {request.Id}");

            // Busca o produto pelo ID na camada de aplicação
            var product = await _productService.GetProductByIdAsync(Guid.Parse(request.Id));

            if (product == null)
            {
                // Log de produto não encontrado
                Console.WriteLine($"Produto com ID {request.Id} não encontrado.");

                // Retorna um erro caso o produto não seja encontrado
                throw new RpcException(new Status(StatusCode.NotFound, "Produto não encontrado."));
            }

            // Log de sucesso
            Console.WriteLine($"Produto encontrado: {product.Name}");

            // Mapeia o resultado para o tipo gRPC
            return new ProductResponse
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                Stock = product.Stock
            };
        }

        /// <summary>
        /// Lista todos os produtos disponíveis.
        /// </summary>
        /// <param name="request">Requisição vazia.</param>
        /// <param name="context">Contexto da chamada gRPC.</param>
        /// <returns>Resposta contendo a lista de produtos.</returns>
        public override async Task<ProductListResponse> ListProducts(EmptyRequest request, ServerCallContext context)
        {
            // Log de início do método
            Console.WriteLine("Recebida solicitação para listar todos os produtos.");

            // Obtém todos os produtos da camada de aplicação
            var products = await _productService.GetAllProductsAsync();

            // Log do número de produtos encontrados
            Console.WriteLine($"Total de produtos encontrados: {products.Count()}");

            // Mapeia os produtos para o tipo gRPC
            return new ProductListResponse
            {
                Products = { products.Select(p => new ProductResponse
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Description = p.Description,
                    Price = (double)p.Price,
                    Stock = p.Stock
                }) }
            };
        }
    }
}