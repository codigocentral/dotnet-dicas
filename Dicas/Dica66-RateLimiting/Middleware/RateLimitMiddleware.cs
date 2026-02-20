using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Dica66_RateLimiting.Middleware;

public class RateLimitMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMetricsMiddleware> _logger;

    public RateLimitMetricsMiddleware(RequestDelegate next, ILogger<RateLimitMetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Adiciona headers informativos sobre rate limiting
        context.Response.OnStarting(() =>
        {
            // Adiciona informações sobre políticas de rate limiting disponíveis
            if (!context.Response.Headers.ContainsKey("X-RateLimit-Policies"))
            {
                context.Response.Headers["X-RateLimit-Policies"] = "PerIP,PerUser,PerTier,ConcurrentOperations";
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public class CustomRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomRateLimitMiddleware> _logger;
    private static readonly Dictionary<string, (int Count, DateTime LastReset)> _requestCounts = new();
    private static readonly object _lock = new();

    public CustomRateLimitMiddleware(RequestDelegate next, ILogger<CustomRateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Implementação customizada de rate limiting para demonstração
        var endpoint = context.Request.Path.Value;
        
        if (endpoint?.StartsWith("/api/custom") == true)
        {
            var clientId = GetClientIdentifier(context);
            var isAllowed = CheckRateLimit(clientId, 5, TimeSpan.FromMinutes(1));

            if (!isAllowed)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
                
                context.Response.StatusCode = 429;
                context.Response.Headers["X-RateLimit-Custom"] = "true";
                context.Response.Headers["X-RateLimit-Limit"] = "5";
                context.Response.Headers["X-RateLimit-Window"] = "60";
                
                await context.Response.WriteAsync("""
                    {
                        "success": false,
                        "message": "Custom rate limit exceeded. Maximum 5 requests per minute for custom endpoints.",
                        "statusCode": 429,
                        "timestamp": "{DateTime.UtcNow:O}"
                    }
                    """);
                return;
            }
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        return context.User.Identity?.Name 
               ?? context.Connection.RemoteIpAddress?.ToString() 
               ?? "unknown";
    }

    private static bool CheckRateLimit(string clientId, int limit, TimeSpan window)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            
            if (_requestCounts.TryGetValue(clientId, out var entry))
            {
                if (now - entry.LastReset > window)
                {
                    // Reset da janela
                    _requestCounts[clientId] = (1, now);
                    return true;
                }
                
                if (entry.Count >= limit)
                {
                    return false;
                }
                
                _requestCounts[clientId] = (entry.Count + 1, entry.LastReset);
                return true;
            }
            
            _requestCounts[clientId] = (1, now);
            return true;
        }
    }
}
