using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Dica67_APIVersioning.Models;

namespace Dica67_APIVersioning.Controllers.Alternative;

/// <summary>
/// Orders API - Demonstra versionamento por Header
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ILogger<OrdersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém pedidos - Versionamento por Header (X-API-Version)
    /// V1: Lista básica
    /// V2: Lista com detalhes de entrega
    /// </summary>
    /// <returns>Lista de pedidos baseada na versão do header</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public ActionResult<ApiResponse<object>> GetOrders()
    {
        var requestedVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        _logger.LogInformation("Solicitação de pedidos - Versão por Header: {Version}", requestedVersion);

        if (requestedVersion == "2.0")
        {
            // V2: Inclui informações de entrega
            var ordersV2 = new[]
            {
                new
                {
                    Id = 1,
                    CustomerName = "João Silva",
                    TotalAmount = 299.99m,
                    Status = "Shipped",
                    OrderDate = DateTime.UtcNow.AddDays(-5),
                    // Novos campos na V2
                    DeliveryInfo = new
                    {
                        EstimatedDelivery = DateTime.UtcNow.AddDays(2),
                        TrackingCode = "BR123456789",
                        DeliveryMethod = "Express"
                    },
                    PaymentInfo = new
                    {
                        Method = "Credit Card",
                        LastFourDigits = "1234"
                    }
                }
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = ordersV2,
                Message = "Pedidos V2 obtidos com sucesso (com informações de entrega)",
                ApiVersion = "2.0",
                Metadata = new Dictionary<string, object>
                {
                    { "versioning_method", "header" },
                    { "header_name", "X-API-Version" },
                    { "v2_features", new[] { "delivery_info", "payment_info", "tracking" } }
                }
            });
        }

        // V1: Lista básica
        var ordersV1 = new[]
        {
            new
            {
                Id = 1,
                CustomerName = "João Silva",
                TotalAmount = 299.99m,
                Status = "Shipped",
                OrderDate = DateTime.UtcNow.AddDays(-5)
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = ordersV1,
            Message = "Pedidos V1 obtidos com sucesso (versão básica)",
            ApiVersion = "1.0",
            Metadata = new Dictionary<string, object>
            {
                { "versioning_method", "header" },
                { "header_name", "X-API-Version" },
                { "note", "Use header 'X-API-Version: 2.0' para versão com mais detalhes" }
            }
        });
    }
}

/// <summary>
/// Customers API - Demonstra versionamento por Query Parameter
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiVersion("3.0")]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ILogger<CustomersController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém clientes - Versionamento por Query Parameter (?api-version=)
    /// V1: Dados básicos
    /// V2: Inclui endereço
    /// V3: Inclui preferências e histórico
    /// </summary>
    /// <returns>Lista de clientes baseada na versão do query parameter</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [MapToApiVersion("3.0")]
    public ActionResult<ApiResponse<object>> GetCustomers()
    {
        var requestedVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        _logger.LogInformation("Solicitação de clientes - Versão por Query: {Version}", requestedVersion);

        var baseCustomer = new
        {
            Id = 1,
            Name = "Maria Santos",
            Email = "maria@email.com",
            Phone = "+55 11 99999-9999"
        };

        switch (requestedVersion)
        {
            case "3.0":
                var customerV3 = new
                {
                    baseCustomer.Id,
                    baseCustomer.Name,
                    baseCustomer.Email,
                    baseCustomer.Phone,
                    // V2 fields
                    Address = new
                    {
                        Street = "Rua das Flores, 123",
                        City = "São Paulo",
                        State = "SP",
                        ZipCode = "01234-567",
                        Country = "Brasil"
                    },
                    // V3 fields
                    Preferences = new
                    {
                        Language = "pt-BR",
                        Currency = "BRL",
                        Newsletter = true,
                        NotificationMethods = new[] { "email", "sms" }
                    },
                    Statistics = new
                    {
                        TotalOrders = 15,
                        TotalSpent = 2599.99m,
                        LastOrderDate = DateTime.UtcNow.AddDays(-10),
                        CustomerSince = DateTime.UtcNow.AddYears(-2)
                    }
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new[] { customerV3 },
                    Message = "Clientes V3 obtidos com sucesso (dados completos)",
                    ApiVersion = "3.0",
                    Metadata = new Dictionary<string, object>
                    {
                        { "versioning_method", "query_parameter" },
                        { "parameter_name", "api-version" },
                        { "v3_features", new[] { "preferences", "statistics", "complete_profile" } }
                    }
                });

            case "2.0":
                var customerV2 = new
                {
                    baseCustomer.Id,
                    baseCustomer.Name,
                    baseCustomer.Email,
                    baseCustomer.Phone,
                    Address = new
                    {
                        Street = "Rua das Flores, 123",
                        City = "São Paulo",
                        State = "SP",
                        ZipCode = "01234-567",
                        Country = "Brasil"
                    }
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new[] { customerV2 },
                    Message = "Clientes V2 obtidos com sucesso (com endereço)",
                    ApiVersion = "2.0",
                    Metadata = new Dictionary<string, object>
                    {
                        { "versioning_method", "query_parameter" },
                        { "parameter_name", "api-version" },
                        { "v2_features", new[] { "address_info" } }
                    }
                });

            default: // V1
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new[] { baseCustomer },
                    Message = "Clientes V1 obtidos com sucesso (dados básicos)",
                    ApiVersion = "1.0",
                    Metadata = new Dictionary<string, object>
                    {
                        { "versioning_method", "query_parameter" },
                        { "parameter_name", "api-version" },
                        { "note", "Use ?api-version=2.0 ou ?api-version=3.0 para mais detalhes" }
                    }
                });
        }
    }
}
