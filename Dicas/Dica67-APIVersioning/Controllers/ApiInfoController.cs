using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Dica67_APIVersioning.Models;

namespace Dica67_APIVersioning.Controllers;

/// <summary>
/// API Information Controller - Fornece informações sobre versionamento
/// </summary>
[ApiController]
[Route("api/info")]
[Produces("application/json")]
public class ApiInfoController : ControllerBase
{
    private readonly ILogger<ApiInfoController> _logger;

    public ApiInfoController(ILogger<ApiInfoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém informações sobre as versões disponíveis da API
    /// </summary>
    /// <returns>Informações detalhadas sobre versionamento</returns>
    [HttpGet("versions")]
    public ActionResult<ApiResponse<object>> GetVersionInfo()
    {
        _logger.LogInformation("Solicitação de informações de versionamento");

        var versionInfo = new
        {
            CurrentVersion = "3.0",
            SupportedVersions = new[] { "1.0", "2.0", "3.0" },
            DefaultVersion = "1.0",
            DeprecatedVersions = Array.Empty<string>(),
            
            VersioningStrategies = new
            {
                UrlPath = new
                {
                    Description = "Versão especificada na URL",
                    Example = "/api/v1.0/products",
                    Endpoints = new[] { "/api/v{version}/products" },
                    Pros = new[] { "Claro e explícito", "Fácil de cachear", "SEO friendly" },
                    Cons = new[] { "URLs diferentes para cada versão", "Pode gerar confusion" }
                },
                
                Header = new
                {
                    Description = "Versão especificada via header HTTP",
                    Example = "X-API-Version: 2.0",
                    Endpoints = new[] { "/api/orders" },
                    Pros = new[] { "URLs limpos", "Flexível", "Não polui URL" },
                    Cons = new[] { "Menos óbvio", "Pode ser esquecido", "Dificulta debug" }
                },
                
                QueryParameter = new
                {
                    Description = "Versão especificada via query parameter",
                    Example = "?api-version=3.0",
                    Endpoints = new[] { "/api/customers" },
                    Pros = new[] { "Visível na URL", "Fácil de testar", "Compatível com REST" },
                    Cons = new[] { "URLs mais longas", "Pode conflitar com outros params" }
                }
            },
            
            VersionDetails = new
            {
                V1_0 = new
                {
                    ReleaseDate = "2024-01-15",
                    Status = "Stable",
                    Features = new[] { "Basic CRUD operations", "Simple product model" },
                    BreakingChanges = Array.Empty<string>(),
                    Endpoints = new[] { "/api/v1.0/products" }
                },
                
                V2_0 = new
                {
                    ReleaseDate = "2024-06-10",
                    Status = "Stable",
                    Features = new[] { "Categories support", "Product ratings", "Inventory tracking", "Tags system" },
                    BreakingChanges = new[] { "Category field now required" },
                    Endpoints = new[] { "/api/v2.0/products", "/api/orders (header)", "/api/customers (query)" }
                },
                
                V3_0 = new
                {
                    ReleaseDate = "2024-12-01",
                    Status = "Stable",
                    Features = new[] { "SKU-based identification", "Money object", "Product search", "Stock management", "Rich metadata" },
                    BreakingChanges = new[] { "ID changed to SKU", "Name changed to Title", "Description changed to Summary", "Price is now Money object" },
                    Endpoints = new[] { "/api/v3.0/products", "/api/customers?api-version=3.0" }
                }
            },
            
            MigrationGuide = new
            {
                V1_to_V2 = new
                {
                    Difficulty = "Low",
                    Changes = new[] { "Add category field to product creation", "Handle new response fields" },
                    Timeline = "1-2 days"
                },
                
                V2_to_V3 = new
                {
                    Difficulty = "High",
                    Changes = new[] { "Update ID references to SKU", "Handle name->title mapping", "Update price handling", "Adapt to new response structure" },
                    Timeline = "1-2 weeks"
                }
            },
            
            TestingEndpoints = new
            {
                Products_V1 = "/api/v1.0/products",
                Products_V2 = "/api/v2.0/products",
                Products_V3 = "/api/v3.0/products",
                Orders_Header = "/api/orders (use X-API-Version header)",
                Customers_Query = "/api/customers?api-version=2.0"
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = versionInfo,
            Message = "Informações de versionamento obtidas com sucesso",
            ApiVersion = "N/A",
            Metadata = new Dictionary<string, object>
            {
                { "documentation_url", "/swagger" },
                { "contact", "api-support@example.com" },
                { "last_updated", DateTime.UtcNow.ToString("yyyy-MM-dd") }
            }
        });
    }

    /// <summary>
    /// Demonstra comparação entre versões
    /// </summary>
    /// <returns>Comparação lado a lado das versões</returns>
    [HttpGet("comparison")]
    public ActionResult<ApiResponse<object>> GetVersionComparison()
    {
        _logger.LogInformation("Solicitação de comparação entre versões");

        var comparison = new
        {
            ProductModel = new
            {
                V1 = new
                {
                    Fields = new[] { "Id (int)", "Name (string)", "Price (decimal)", "Description (string)", "CreatedAt (DateTime)" },
                    Endpoints = new[] { "GET /products", "GET /products/{id}", "POST /products" }
                },
                
                V2 = new
                {
                    Fields = new[] { "Id (int)", "Name (string)", "Price (decimal)", "Description (string)", "CreatedAt (DateTime)", "Category (string)", "Tags (string[])", "Rating (object)", "Inventory (object)" },
                    Endpoints = new[] { "GET /products", "GET /products/{id}", "GET /products/category/{category}", "POST /products" }
                },
                
                V3 = new
                {
                    Fields = new[] { "Sku (string)", "Title (string)", "Price (Money object)", "Summary (string)", "Metadata (object)", "Availability (object)" },
                    Endpoints = new[] { "GET /products", "GET /products/{sku}", "GET /products/search", "POST /products", "PATCH /products/{sku}/stock" }
                }
            },
            
            BreakingChanges = new
            {
                V1_to_V2 = new[] { "Category field required for creation" },
                V2_to_V3 = new[] { "ID -> SKU", "Name -> Title", "Description -> Summary", "Price -> Money object", "Complete response structure change" }
            },
            
            BackwardCompatibility = new
            {
                V1 = "Fully supported",
                V2 = "Fully supported", 
                V3 = "No backward compatibility with V1/V2 due to breaking changes"
            },
            
            RecommendedMigrationPath = new[]
            {
                "1. Start with V1 for simple use cases",
                "2. Migrate to V2 when you need categories and ratings",
                "3. Plan carefully before moving to V3 due to breaking changes",
                "4. Consider running multiple versions in parallel during migration"
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = comparison,
            Message = "Comparação entre versões obtida com sucesso",
            ApiVersion = "N/A"
        });
    }

    /// <summary>
    /// Testa a detecção automática de versão
    /// </summary>
    /// <returns>Informações sobre como a versão foi detectada</returns>
    [HttpGet("version-detection")]
    public ActionResult<ApiResponse<object>> TestVersionDetection()
    {
        var detectedVersion = HttpContext.GetRequestedApiVersion();
        var versionSource = "Default (1.0)";

        // Verificar diferentes fontes de versão
        if (Request.Headers.ContainsKey("X-API-Version"))
        {
            versionSource = $"Header: {Request.Headers["X-API-Version"]}";
        }
        else if (Request.Query.ContainsKey("api-version"))
        {
            versionSource = $"Query Parameter: {Request.Query["api-version"]}";
        }
        else if (Request.Path.Value?.Contains("/v") == true)
        {
            versionSource = $"URL Path: {Request.Path}";
        }

        var detection = new
        {
            DetectedVersion = detectedVersion?.ToString() ?? "None",
            VersionSource = versionSource,
            RequestPath = Request.Path.Value,
            Headers = new
            {
                XApiVersion = Request.Headers.ContainsKey("X-API-Version") ? Request.Headers["X-API-Version"].ToString() : null,
                Accept = Request.Headers.Accept.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            },
            QueryParameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
            Method = Request.Method,
            ContentType = Request.ContentType
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = detection,
            Message = "Detecção de versão analisada com sucesso",
            ApiVersion = detectedVersion?.ToString() ?? "Unknown"
        });
    }
}
