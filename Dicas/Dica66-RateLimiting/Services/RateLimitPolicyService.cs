using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Dica66_RateLimiting.Services;

public class RateLimitPolicyService
{
    public static void ConfigureRateLimitPolicies(IServiceCollection services)
    {
        services.AddRateLimiter(limiterOptions =>
        {
            // 1. Política Global - Fixed Window
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // 2. Política Por IP - Sliding Window  
            limiterOptions.AddPolicy("PerIP", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new SlidingWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4
                    }));

            // 3. Política Por Usuário - Token Bucket
            limiterOptions.AddPolicy("PerUser", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
                    factory: partition => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 50,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                        TokensPerPeriod = 10,
                        AutoReplenishment = true
                    }));

            // 4. Política Concorrente - Para operações que consomem recursos
            limiterOptions.AddPolicy("ConcurrentOperations", httpContext =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));

            // 5. Política por Tier de Usuário - Dinâmica
            limiterOptions.AddPolicy("PerTier", httpContext =>
            {
                var userTier = GetUserTier(httpContext);
                
                return userTier switch
                {
                    "Free" => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"free_{httpContext.User.Identity?.Name ?? "anonymous"}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        }),
                    
                    "Premium" => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"premium_{httpContext.User.Identity?.Name ?? "anonymous"}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }),
                    
                    "Enterprise" => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"enterprise_{httpContext.User.Identity?.Name ?? "anonymous"}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 1000,
                            Window = TimeSpan.FromMinutes(1)
                        }),
                    
                    _ => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: "default",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        })
                };
            });

            // Configuração de resposta quando limite é excedido
            limiterOptions.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.Headers["X-RateLimit-Policy"] = context.HttpContext.Request.Headers["X-RateLimit-Policy"].FirstOrDefault() ?? "Global";
                
                await context.HttpContext.Response.WriteAsync("""
                    {
                        "success": false,
                        "message": "Rate limit exceeded. Too many requests.",
                        "statusCode": 429,
                        "timestamp": "{DateTime.UtcNow:O}"
                    }
                    """, cancellationToken: token);
            };
        });
    }

    private static string GetUserTier(HttpContext httpContext)
    {
        // Simula a obtenção do tier do usuário
        // Em um cenário real, isso viria de claims, banco de dados, etc.
        
        if (httpContext.Request.Headers.TryGetValue("X-User-Tier", out var tierHeader))
        {
            return tierHeader.FirstOrDefault() ?? "Free";
        }

        // Baseado no nome do usuário para demonstração
        var userName = httpContext.User.Identity?.Name;
        return userName switch
        {
            var name when name?.Contains("premium") == true => "Premium",
            var name when name?.Contains("enterprise") == true => "Enterprise",
            _ => "Free"
        };
    }
}
