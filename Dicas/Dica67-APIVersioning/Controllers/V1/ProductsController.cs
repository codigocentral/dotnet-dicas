using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Dica67_APIVersioning.Models;
using Dica67_APIVersioning.Services;

namespace Dica67_APIVersioning.Controllers.V1;

/// <summary>
/// Products API V1 - Versão básica com funcionalidades essenciais
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos (V1)
    /// </summary>
    /// <returns>Lista de produtos</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> GetProducts()
    {
        _logger.LogInformation("Solicitação V1: Listando todos os produtos");

        var products = await _productService.GetAllProductsAsync();
        
        return Ok(new ApiResponse<IEnumerable<Product>>
        {
            Success = true,
            Data = products,
            Message = "Produtos obtidos com sucesso",
            ApiVersion = "1.0",
            Metadata = new Dictionary<string, object>
            {
                { "total_count", products.Count() },
                { "version_features", new[] { "basic_crud", "simple_product_model" } }
            }
        });
    }

    /// <summary>
    /// Obtém um produto específico por ID (V1)
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Produto encontrado</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<Product>>> GetProduct(int id)
    {
        _logger.LogInformation("Solicitação V1: Buscando produto {ProductId}", id);

        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
        {
            return NotFound(new ApiResponse<Product>
            {
                Success = false,
                Message = $"Produto com ID {id} não encontrado",
                ApiVersion = "1.0"
            });
        }

        return Ok(new ApiResponse<Product>
        {
            Success = true,
            Data = product,
            Message = "Produto encontrado com sucesso",
            ApiVersion = "1.0"
        });
    }

    /// <summary>
    /// Cria um novo produto (V1)
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <returns>Produto criado</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Product>>> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("Solicitação V1: Criando produto {ProductName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ApiResponse<Product>
            {
                Success = false,
                Message = "Nome do produto é obrigatório",
                ApiVersion = "1.0"
            });
        }

        var product = await _productService.CreateProductAsync(request);
        
        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            new ApiResponse<Product>
            {
                Success = true,
                Data = product,
                Message = "Produto criado com sucesso",
                ApiVersion = "1.0"
            });
    }
}
