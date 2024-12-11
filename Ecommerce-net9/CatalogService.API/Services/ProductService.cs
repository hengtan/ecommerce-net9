using Grpc.Core;
using CatalogService.Application.Interfaces;
using CatalogService.Application.DTOs;
using Serilog;
using Serilog.Sinks.Splunk;

namespace CatalogService.API.Services
{
    public class ProductService : ProductService.ProductServiceBase
    {
        private readonly IProductService _productService;

        // Injeção de dependência para acessar a camada de aplicação
        public ProductService(IProductService productService)
        {
            _productService = productService;
        }

        // Implementação do método GetProductById
        public override async Task<ProductResponse> GetProductById(ProductRequest request, ServerCallContext context)
        {
            // Busca o produto pelo ID na camada de aplicação
            var product = await _productService.GetProductByIdAsync(Guid.Parse(request.Id));

            if (product == null)
            {
                // Retorna um erro caso o produto não seja encontrado
                throw new RpcException(new Status(StatusCode.NotFound, "Produto não encontrado."));
            }

            // Mapeia o resultado para o tipo gRPC
            return new ProductResponse
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            };
        }

        // Implementação do método ListProducts
        public override async Task<ProductListResponse> ListProducts(EmptyRequest request, ServerCallContext context)
        {
            // Obtém todos os produtos da camada de aplicação
            var products = await _productService.GetAllProductsAsync();

            // Mapeia os produtos para o tipo gRPC
            return new ProductListResponse
            {
                Products = { products.Select(p => new ProductResponse
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock
                }) }
            };
        }
    }
}