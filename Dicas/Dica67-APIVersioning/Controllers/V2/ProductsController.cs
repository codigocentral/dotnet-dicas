using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Dica67_APIVersioning.Models;
using Dica67_APIVersioning.Services;

namespace Dica67_APIVersioning.Controllers.V2;

/// <summary>
/// Products API V2 - Versão aprimorada com categorias, tags e inventário
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductServiceV2 _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductServiceV2 productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos com informações expandidas (V2)
    /// </summary>
    /// <returns>Lista de produtos com categorias, ratings e inventário</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductV2>>>> GetProducts()
    {
        _logger.LogInformation("Solicitação V2: Listando todos os produtos com informações expandidas");

        var products = await _productService.GetAllProductsAsync();
        
        return Ok(new ApiResponse<IEnumerable<ProductV2>>
        {
            Success = true,
            Data = products,
            Message = "Produtos V2 obtidos com sucesso",
            ApiVersion = "2.0",
            Metadata = new Dictionary<string, object>
            {
                { "total_count", products.Count() },
                { "version_features", new[] { "categories", "tags", "ratings", "inventory" } },
                { "categories", products.Select(p => p.Category).Distinct().ToArray() }
            }
        });
    }

    /// <summary>
    /// Obtém um produto específico por ID com informações expandidas (V2)
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Produto com informações completas</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductV2>>> GetProduct(int id)
    {
        _logger.LogInformation("Solicitação V2: Buscando produto {ProductId} com informações expandidas", id);

        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
        {
            return NotFound(new ApiResponse<ProductV2>
            {
                Success = false,
                Message = $"Produto com ID {id} não encontrado",
                ApiVersion = "2.0"
            });
        }

        return Ok(new ApiResponse<ProductV2>
        {
            Success = true,
            Data = product,
            Message = "Produto V2 encontrado com sucesso",
            ApiVersion = "2.0",
            Metadata = new Dictionary<string, object>
            {
                { "availability_status", product.Inventory?.Available > 0 ? "available" : "out_of_stock" },
                { "rating_category", product.Rating?.Average >= 4.0 ? "excellent" : "good" }
            }
        });
    }

    /// <summary>
    /// Obtém produtos por categoria (Novo na V2)
    /// </summary>
    /// <param name="category">Nome da categoria</param>
    /// <returns>Lista de produtos da categoria especificada</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductV2>>>> GetProductsByCategory(string category)
    {
        _logger.LogInformation("Solicitação V2: Buscando produtos da categoria {Category}", category);

        var products = await _productService.GetProductsByCategoryAsync(category);
        
        return Ok(new ApiResponse<IEnumerable<ProductV2>>
        {
            Success = true,
            Data = products,
            Message = $"Produtos da categoria '{category}' obtidos com sucesso",
            ApiVersion = "2.0",
            Metadata = new Dictionary<string, object>
            {
                { "category", category },
                { "count", products.Count() },
                { "avg_rating", products.Any() ? products.Average(p => p.Rating?.Average ?? 0) : 0 }
            }
        });
    }

    /// <summary>
    /// Cria um novo produto com informações expandidas (V2)
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <returns>Produto criado com informações completas</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductV2>>> CreateProduct([FromBody] CreateProductV2Request request)
    {
        _logger.LogInformation("Solicitação V2: Criando produto {ProductName} na categoria {Category}", 
            request.Name, request.Category);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiResponse<ProductV2>
            {
                Success = false,
                Message = "Nome do produto é obrigatório",
                ApiVersion = "2.0"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest(new ApiResponse<ProductV2>
            {
                Success = false,
                Message = "Categoria é obrigatória na V2",
                ApiVersion = "2.0"
            });
        }

        var product = await _productService.CreateProductAsync(request);
        
        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            new ApiResponse<ProductV2>
            {
                Success = true,
                Data = product,
                Message = "Produto V2 criado com sucesso",
                ApiVersion = "2.0",
                Metadata = new Dictionary<string, object>
                {
                    { "features_added", new[] { "category", "tags", "rating_system", "inventory_tracking" } }
                }
            });
    }
}
