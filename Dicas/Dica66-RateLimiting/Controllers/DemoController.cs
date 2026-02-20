using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dica66_RateLimiting.Models;

namespace Dica66_RateLimiting.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;

    public DemoController(ILogger<DemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Demonstra diferentes cenários de rate limiting
    /// </summary>
    [HttpGet("scenarios")]
    public ActionResult<ApiResponse<object>> GetRateLimitingScenarios()
    {
        var scenarios = new
        {
            Scenarios = new[]
            {
                new { 
                    Name = "Per IP", 
                    Endpoint = "/api/weather", 
                    Policy = "PerIP",
                    Limit = "20 requests per minute",
                    Type = "Sliding Window",
                    Description = "Limita requisições por endereço IP usando janela deslizante"
                },
                new { 
                    Name = "Per User", 
                    Endpoint = "/api/weather/detailed", 
                    Policy = "PerUser",
                    Limit = "50 tokens, refill 10 every 30s",
                    Type = "Token Bucket",
                    Description = "Limita requisições por usuário usando token bucket"
                },
                new { 
                    Name = "Per Tier", 
                    Endpoint = "/api/weather/premium", 
                    Policy = "PerTier",
                    Limit = "Free: 10/min, Premium: 100/min, Enterprise: 1000/min",
                    Type = "Fixed Window",
                    Description = "Limita baseado no tier do usuário"
                },
                new { 
                    Name = "Concurrent Operations", 
                    Endpoint = "/api/resource/heavy-operation", 
                    Policy = "ConcurrentOperations",
                    Limit = "5 concurrent requests",
                    Type = "Concurrency Limiter",
                    Description = "Limita número de operações simultâneas"
                },
                new { 
                    Name = "Custom Middleware", 
                    Endpoint = "/api/resource/custom-limit", 
                    Policy = "Custom",
                    Limit = "5 requests per minute",
                    Type = "Custom Implementation",
                    Description = "Rate limiting customizado via middleware"
                }
            },
            Instructions = new
            {
                TestingTips = new[]
                {
                    "Use diferentes IPs ou headers X-User-Tier para testar cenários",
                    "Faça múltiplas requisições rapidamente para triggerar o rate limit",
                    "Observe os headers de resposta para informações do rate limit",
                    "Use ferramentas como curl ou Postman para teste automatizado"
                },
                Headers = new
                {
                    UserTier = "X-User-Tier: Free|Premium|Enterprise",
                    Example = "curl -H 'X-User-Tier: Premium' http://localhost:5000/api/weather/premium"
                }
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = scenarios,
            Message = "Cenários de rate limiting disponíveis"
        });
    }

    /// <summary>
    /// Endpoint para testar rapidamente o rate limiting
    /// </summary>
    [HttpGet("test/{count:int}")]
    [EnableRateLimiting("PerIP")]
    public ActionResult<ApiResponse<object>> TestRateLimit(int count)
    {
        _logger.LogInformation("Teste de rate limit - requisição {Count}", count);

        var result = new
        {
            RequestNumber = count,
            Timestamp = DateTime.UtcNow,
            Message = $"Esta é a requisição número {count}",
            RateLimitHeaders = new
            {
                Policy = "PerIP",
                Limit = 20,
                Window = "1 minute"
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = result,
            Message = $"Requisição {count} processada com sucesso"
        });
    }

    /// <summary>
    /// Status das políticas de rate limiting
    /// </summary>
    [HttpGet("status")]
    public ActionResult<ApiResponse<object>> GetRateLimitStatus()
    {
        var status = new
        {
            GlobalPolicy = new
            {
                Type = "Fixed Window",
                Limit = 100,
                Window = "1 minute",
                PartitionBy = "User or IP"
            },
            Policies = new[]
            {
                new { Name = "PerIP", Type = "Sliding Window", Active = true },
                new { Name = "PerUser", Type = "Token Bucket", Active = true },
                new { Name = "PerTier", Type = "Fixed Window", Active = true },
                new { Name = "ConcurrentOperations", Type = "Concurrency", Active = true }
            },
            Middleware = new
            {
                RateLimitMetrics = true,
                CustomRateLimit = true
            },
            SystemTime = DateTime.UtcNow
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = status,
            Message = "Status do sistema de rate limiting"
        });
    }
}
