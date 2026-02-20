using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dica66_RateLimiting.Models;

namespace Dica66_RateLimiting.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherController> _logger;

    public WeatherController(ILogger<WeatherController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém previsão do tempo - Rate limit por IP (20 req/min)
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("PerIP")]
    public ActionResult<ApiResponse<IEnumerable<WeatherForecast>>> GetWeatherForecast()
    {
        _logger.LogInformation("Solicitação de previsão do tempo recebida");

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        return Ok(new ApiResponse<IEnumerable<WeatherForecast>>
        {
            Success = true,
            Data = forecasts,
            Message = "Previsão do tempo obtida com sucesso",
            RateLimit = GetRateLimitInfo("PerIP", 20, TimeSpan.FromMinutes(1))
        });
    }

    /// <summary>
    /// Obtém previsão detalhada - Rate limit por usuário (Token Bucket)
    /// </summary>
    [HttpGet("detailed")]
    [EnableRateLimiting("PerUser")]
    public async Task<ActionResult<ApiResponse<WeatherForecast>>> GetDetailedWeatherForecast()
    {
        _logger.LogInformation("Solicitação de previsão detalhada recebida");

        // Simula processamento mais demorado
        await Task.Delay(500);

        var forecast = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };

        return Ok(new ApiResponse<WeatherForecast>
        {
            Success = true,
            Data = forecast,
            Message = "Previsão detalhada obtida com sucesso",
            RateLimit = GetRateLimitInfo("PerUser", 50, TimeSpan.FromSeconds(30))
        });
    }

    /// <summary>
    /// Endpoint premium com rate limit baseado no tier do usuário
    /// </summary>
    [HttpGet("premium")]
    [EnableRateLimiting("PerTier")]
    public ActionResult<ApiResponse<object>> GetPremiumWeatherData()
    {
        _logger.LogInformation("Solicitação de dados premium recebida");

        var userTier = GetUserTier();
        var premiumData = new
        {
            ExtendedForecast = Enumerable.Range(1, 30).Select(index => new
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Temperature = Random.Shared.Next(-20, 55),
                Humidity = Random.Shared.Next(30, 90),
                WindSpeed = Random.Shared.Next(0, 30)
            }),
            UserTier = userTier,
            Features = userTier switch
            {
                "Free" => new[] { "Basic forecast" },
                "Premium" => new[] { "Extended forecast", "Humidity data", "Wind data" },
                "Enterprise" => new[] { "Extended forecast", "Humidity data", "Wind data", "Historical data", "API access" },
                _ => new[] { "Limited access" }
            }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = premiumData,
            Message = $"Dados premium para tier {userTier} obtidos com sucesso",
            RateLimit = GetRateLimitInfo("PerTier", GetTierLimit(userTier), TimeSpan.FromMinutes(1))
        });
    }

    private string GetUserTier()
    {
        if (Request.Headers.TryGetValue("X-User-Tier", out var tierHeader))
        {
            return tierHeader.FirstOrDefault() ?? "Free";
        }

        var userName = User.Identity?.Name;
        return userName switch
        {
            var name when name?.Contains("premium") == true => "Premium",
            var name when name?.Contains("enterprise") == true => "Enterprise",
            _ => "Free"
        };
    }

    private static int GetTierLimit(string tier)
    {
        return tier switch
        {
            "Free" => 10,
            "Premium" => 100,
            "Enterprise" => 1000,
            _ => 5
        };
    }

    private static RateLimitInfo GetRateLimitInfo(string policy, int limit, TimeSpan window)
    {
        return new RateLimitInfo
        {
            Policy = policy,
            Limit = limit,
            Remaining = Random.Shared.Next(0, limit), // Em implementação real, isso viria do rate limiter
            ResetTime = DateTime.UtcNow.Add(window)
        };
    }
}
