using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Dica67_APIVersioning.Models;
using Dica67_APIVersioning.Services;

namespace Dica67_APIVersioning.Controllers.V3;

/// <summary>
/// Products API V3 - Versão moderna com mudanças breaking e novas funcionalidades
/// </summary>
[ApiController]
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductServiceV3 _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductServiceV3 productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os produtos com modelo modernizado (V3)
    /// </summary>
    /// <returns>Lista de produtos com SKU, metadata e disponibilidade</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductV3>>>> GetProducts()
    {
        _logger.LogInformation("Solicitação V3: Listando todos os produtos com modelo modernizado");

        var products = await _productService.GetAllProductsAsync();
        
        return Ok(new ApiResponse<IEnumerable<ProductV3>>
        {
            Success = true,
            Data = products,
            Message = "Produtos V3 obtidos com sucesso",
            ApiVersion = "3.0",
            Metadata = new Dictionary<string, object>
            {
                { "total_count", products.Count() },
                { "version_features", new[] { "sku_based", "money_object", "metadata", "availability_tracking" } },
                { "breaking_changes", new[] { "id_to_sku", "name_to_title", "description_to_summary" } },
                { "in_stock_count", products.Count(p => p.Availability.InStock) }
            }
        });
    }

    /// <summary>
    /// Obtém um produto específico por SKU (V3 - Breaking Change)
    /// </summary>
    /// <param name="sku">SKU do produto</param>
    /// <returns>Produto com modelo modernizado</returns>
    [HttpGet("{sku}")]
    public async Task<ActionResult<ApiResponse<ProductV3>>> GetProduct(string sku)
    {
        _logger.LogInformation("Solicitação V3: Buscando produto {ProductSku}", sku);

        var product = await _productService.GetProductBySkuAsync(sku);
        
        if (product == null)
        {
            return NotFound(new ApiResponse<ProductV3>
            {
                Success = false,
                Message = $"Produto com SKU {sku} não encontrado",
                ApiVersion = "3.0"
            });
        }

        return Ok(new ApiResponse<ProductV3>
        {
            Success = true,
            Data = product,
            Message = "Produto V3 encontrado com sucesso",
            ApiVersion = "3.0",
            Metadata = new Dictionary<string, object>
            {
                { "stock_level", product.Availability.Quantity > 10 ? "high" : product.Availability.Quantity > 0 ? "low" : "out" },
                { "brand", product.Metadata.Brand },
                { "days_since_created", (DateTime.UtcNow - product.Metadata.CreatedAt).Days }
            }
        });
    }

    /// <summary>
    /// Busca produtos por texto (Novo na V3)
    /// </summary>
    /// <param name="q">Termo de busca</param>
    /// <returns>Lista de produtos que correspondem ao termo</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductV3>>>> SearchProducts([FromQuery] string q)
    {
        _logger.LogInformation("Solicitação V3: Buscando produtos com termo '{SearchTerm}'", q);

        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ApiResponse<IEnumerable<ProductV3>>
            {
                Success = false,
                Message = "Termo de busca é obrigatório",
                ApiVersion = "3.0"
            });
        }

        var products = await _productService.SearchProductsAsync(q);
        
        return Ok(new ApiResponse<IEnumerable<ProductV3>>
        {
            Success = true,
            Data = products,
            Message = $"Busca por '{q}' realizada com sucesso",
            ApiVersion = "3.0",
            Metadata = new Dictionary<string, object>
            {
                { "search_term", q },
                { "results_count", products.Count() },
                { "search_type", "full_text" }
            }
        });
    }

    /// <summary>
    /// Cria um novo produto com modelo modernizado (V3)
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <returns>Produto criado com modelo V3</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductV3>>> CreateProduct([FromBody] CreateProductV3Request request)
    {
        _logger.LogInformation("Solicitação V3: Criando produto {ProductTitle} da marca {Brand}", 
            request.Title, request.Brand);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ApiResponse<ProductV3>
            {
                Success = false,
                Message = "Título do produto é obrigatório",
                ApiVersion = "3.0"
            });
        }

        if (request.Price.Amount <= 0)
        {
            return BadRequest(new ApiResponse<ProductV3>
            {
                Success = false,
                Message = "Preço deve ser maior que zero",
                ApiVersion = "3.0"
            });
        }

        var product = await _productService.CreateProductAsync(request);
        
        return CreatedAtAction(
            nameof(GetProduct),
            new { sku = product.Sku },
            new ApiResponse<ProductV3>
            {
                Success = true,
                Data = product,
                Message = "Produto V3 criado com sucesso",
                ApiVersion = "3.0",
                Metadata = new Dictionary<string, object>
                {
                    { "generated_sku", product.Sku },
                    { "features_added", new[] { "sku_generation", "money_handling", "availability_tracking", "metadata_system" } },
                    { "breaking_changes_note", "Este endpoint usa o novo modelo V3 com mudanças incompatíveis" }
                }
            });
    }

    /// <summary>
    /// Atualiza o estoque de um produto (Novo na V3)
    /// </summary>
    /// <param name="sku">SKU do produto</param>
    /// <param name="quantity">Nova quantidade em estoque</param>
    /// <returns>Produto com estoque atualizado</returns>
    [HttpPatch("{sku}/stock")]
    public async Task<ActionResult<ApiResponse<ProductV3>>> UpdateStock(string sku, [FromBody] int quantity)
    {
        _logger.LogInformation("Solicitação V3: Atualizando estoque do produto {ProductSku} para {Quantity}", sku, quantity);

        if (quantity < 0)
        {
            return BadRequest(new ApiResponse<ProductV3>
            {
                Success = false,
                Message = "Quantidade não pode ser negativa",
                ApiVersion = "3.0"
            });
        }

        try
        {
            var product = await _productService.UpdateStockAsync(sku, quantity);
            
            return Ok(new ApiResponse<ProductV3>
            {
                Success = true,
                Data = product,
                Message = $"Estoque do produto {sku} atualizado para {quantity} unidades",
                ApiVersion = "3.0",
                Metadata = new Dictionary<string, object>
                {
                    { "previous_stock_status", "unknown" },
                    { "new_stock_status", product.Availability.InStock ? "in_stock" : "out_of_stock" },
                    { "updated_at", product.Metadata.UpdatedAt }
                }
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<ProductV3>
            {
                Success = false,
                Message = ex.Message,
                ApiVersion = "3.0"
            });
        }
    }
}
