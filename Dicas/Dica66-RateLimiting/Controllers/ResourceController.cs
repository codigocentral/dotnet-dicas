using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dica66_RateLimiting.Models;

namespace Dica66_RateLimiting.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly ILogger<ResourceController> _logger;

    public ResourceController(ILogger<ResourceController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Operação que consome muitos recursos - Rate limit de concorrência
    /// </summary>
    [HttpPost("heavy-operation")]
    [EnableRateLimiting("ConcurrentOperations")]
    public async Task<ActionResult<ApiResponse<string>>> ProcessHeavyOperation([FromBody] string data)
    {
        _logger.LogInformation("Iniciando operação pesada para dados: {Data}", data);

        try
        {
            // Simula operação que consome recursos
            await Task.Delay(3000); // 3 segundos
            
            var result = $"Dados processados: {data?.ToUpper()} - Processado em {DateTime.UtcNow}";

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = result,
                Message = "Operação pesada concluída com sucesso",
                RateLimit = new RateLimitInfo
                {
                    Policy = "ConcurrentOperations",
                    Limit = 5,
                    Remaining = Random.Shared.Next(0, 5),
                    ResetTime = DateTime.UtcNow.AddMinutes(1)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante operação pesada");
            
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "Erro interno durante processamento"
            });
        }
    }

    /// <summary>
    /// Endpoint customizado com rate limiting próprio
    /// </summary>
    [HttpGet("custom-limit")]
    public ActionResult<ApiResponse<object>> GetCustomLimitedResource()
    {
        _logger.LogInformation("Acesso a recurso com limite customizado");

        var data = new
        {
            Message = "Este endpoint tem rate limiting customizado implementado via middleware",
            Limit = "5 requests per minute",
            Timestamp = DateTime.UtcNow,
            RequestId = Guid.NewGuid()
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = data,
            Message = "Recurso acessado com sucesso",
            RateLimit = new RateLimitInfo
            {
                Policy = "Custom",
                Limit = 5,
                Remaining = Random.Shared.Next(0, 5),
                ResetTime = DateTime.UtcNow.AddMinutes(1)
            }
        });
    }

    /// <summary>
    /// Endpoint sem rate limiting para comparação
    /// </summary>
    [HttpGet("unlimited")]
    public ActionResult<ApiResponse<object>> GetUnlimitedResource()
    {
        _logger.LogInformation("Acesso a recurso sem limite");

        var data = new
        {
            Message = "Este endpoint não tem rate limiting aplicado",
            Timestamp = DateTime.UtcNow,
            RequestCount = Random.Shared.Next(1, 1000)
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = data,
            Message = "Recurso ilimitado acessado com sucesso"
        });
    }
}
