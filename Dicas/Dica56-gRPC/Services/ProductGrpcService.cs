using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Dica56_gRPC.Models;

namespace Dica56_gRPC.Services;

/// <summary>
/// Serviço gRPC para gerenciamento de produtos
/// Demonstra operações CRUD via gRPC
/// </summary>
public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly IProductRepository _productRepository;

    public ProductGrpcService(ILogger<ProductGrpcService> logger, 
        IProductRepository productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Obter um produto por ID
    /// </summary>
    public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("🔍 Buscando produto ID: {ProductId}", request.Id);

        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Produto com ID {request.Id} não encontrado"));
            }

            _logger.LogInformation("✅ Produto encontrado: {ProductName}", product.Name);
            return product;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar produto ID: {ProductId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno ao buscar produto"));
        }
    }

    /// <summary>
    /// Obter todos os produtos
    /// </summary>
    public override async Task<ProductList> GetProducts(Empty request, ServerCallContext context)
    {
        _logger.LogInformation("📋 Listando todos os produtos");

        try
        {
            var products = await _productRepository.GetAllAsync();
            
            var productList = new ProductList
            {
                TotalCount = products.Count
            };
            
            productList.Products.AddRange(products);

            _logger.LogInformation("✅ Retornados {Count} produtos", products.Count);
            return productList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao listar produtos");
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno ao listar produtos"));
        }
    }

    /// <summary>
    /// Criar um novo produto
    /// </summary>
    public override async Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("➕ Criando novo produto: {ProductName}", request.Name);

        try
        {
            // Validar dados de entrada
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    "Nome do produto é obrigatório"));
            }

            if (request.Price <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    "Preço deve ser maior que zero"));
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                StockQuantity = request.StockQuantity,
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                IsActive = true
            };

            product.Tags.AddRange(request.Tags);

            var createdProduct = await _productRepository.CreateAsync(product);
            
            _logger.LogInformation("✅ Produto criado com ID: {ProductId}", createdProduct.Id);
            return createdProduct;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar produto: {ProductName}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno ao criar produto"));
        }
    }

    /// <summary>
    /// Atualizar um produto existente
    /// </summary>
    public override async Task<Product> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("📝 Atualizando produto ID: {ProductId}", request.Id);

        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            
            if (existingProduct == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Produto com ID {request.Id} não encontrado"));
            }

            // Atualizar campos
            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.Category = request.Category;
            existingProduct.StockQuantity = request.StockQuantity;
            existingProduct.IsActive = request.IsActive;
            existingProduct.UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow);
            
            existingProduct.Tags.Clear();
            existingProduct.Tags.AddRange(request.Tags);

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            
            _logger.LogInformation("✅ Produto atualizado: {ProductName}", updatedProduct.Name);
            return updatedProduct;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao atualizar produto ID: {ProductId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno ao atualizar produto"));
        }
    }

    /// <summary>
    /// Deletar um produto
    /// </summary>
    public override async Task<Empty> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("🗑️ Deletando produto ID: {ProductId}", request.Id);

        try
        {
            var deleted = await _productRepository.DeleteAsync(request.Id);
            
            if (!deleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"Produto com ID {request.Id} não encontrado"));
            }

            _logger.LogInformation("✅ Produto deletado com sucesso ID: {ProductId}", request.Id);
            return new Empty();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao deletar produto ID: {ProductId}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno ao deletar produto"));
        }
    }

    /// <summary>
    /// Buscar produtos
    /// </summary>
    public override async Task<ProductList> SearchProducts(SearchProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("🔍 Buscando produtos: Query='{Query}', Category='{Category}'", 
            request.Query, request.Category);

        try
        {
            var products = await _productRepository.SearchAsync(
                request.Query, 
                request.Category,
                request.MinPrice,
                request.MaxPrice,
                request.Page,
                request.PageSize);

            var productList = new ProductList
            {
                TotalCount = products.Count
            };
            
            productList.Products.AddRange(products);

            _logger.LogInformation("✅ Encontrados {Count} produtos na busca", products.Count);
            return productList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro na busca de produtos");
            throw new RpcException(new Status(StatusCode.Internal, 
                "Erro interno na busca de produtos"));
        }
    }
}
