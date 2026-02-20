using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Dica78.MicroservicesCommunication.Services;

public class HealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly Dictionary<string, Func<Task<HealthCheckResult>>> _healthChecks = new();
    private readonly Timer _healthCheckTimer;
    private readonly Dictionary<string, HealthCheckResult> _lastResults = new();

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        _logger = logger;
        
        // Executar health checks a cada 30 segundos
        _healthCheckTimer = new Timer(ExecuteHealthChecks, null, 
            TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        
        RegisterDefaultHealthChecks();
    }

    private void RegisterDefaultHealthChecks()
    {
        RegisterHealthCheck("database", CheckDatabaseHealth);
        RegisterHealthCheck("redis-cache", CheckRedisHealth);
        RegisterHealthCheck("message-queue", CheckMessageQueueHealth);
        RegisterHealthCheck("external-api", CheckExternalApiHealth);
        RegisterHealthCheck("disk-space", CheckDiskSpaceHealth);
        RegisterHealthCheck("memory", CheckMemoryHealth);
    }

    public void RegisterHealthCheck(string name, Func<Task<HealthCheckResult>> healthCheck)
    {
        _healthChecks[name] = healthCheck;
        _logger.LogInformation("💚 Health check registrado: {HealthCheckName}", name);
    }

    public async Task<Dictionary<string, HealthCheckResult>> ExecuteAllHealthChecks()
    {
        var results = new Dictionary<string, HealthCheckResult>();
        var tasks = _healthChecks.Select(async kvp =>
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await kvp.Value();
                result.ResponseTime = stopwatch.Elapsed;
                results[kvp.Key] = result;
                _lastResults[kvp.Key] = result;
            }
            catch (Exception ex)
            {
                var errorResult = new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = ex.Message,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object> { ["exception"] = ex.GetType().Name }
                };
                results[kvp.Key] = errorResult;
                _lastResults[kvp.Key] = errorResult;
            }
            finally
            {
                stopwatch.Stop();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<HealthCheckResult> ExecuteHealthCheck(string name)
    {
        if (!_healthChecks.TryGetValue(name, out var healthCheck))
        {
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Health check '{name}' não encontrado"
            };
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await healthCheck();
            result.ResponseTime = stopwatch.Elapsed;
            _lastResults[name] = result;
            return result;
        }
        catch (Exception ex)
        {
            var errorResult = new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = ex.Message,
                ResponseTime = stopwatch.Elapsed,
                Data = new Dictionary<string, object> { ["exception"] = ex.GetType().Name }
            };
            _lastResults[name] = errorResult;
            return errorResult;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public OverallHealthStatus GetOverallHealth()
    {
        if (!_lastResults.Any())
        {
            return new OverallHealthStatus
            {
                Status = HealthStatus.Unknown,
                TotalChecks = 0,
                HealthyChecks = 0,
                UnhealthyChecks = 0,
                DegradedChecks = 0
            };
        }

        var healthy = _lastResults.Count(r => r.Value.Status == HealthStatus.Healthy);
        var unhealthy = _lastResults.Count(r => r.Value.Status == HealthStatus.Unhealthy);
        var degraded = _lastResults.Count(r => r.Value.Status == HealthStatus.Degraded);

        var overallStatus = unhealthy > 0 ? HealthStatus.Unhealthy :
                           degraded > 0 ? HealthStatus.Degraded :
                           HealthStatus.Healthy;

        return new OverallHealthStatus
        {
            Status = overallStatus,
            TotalChecks = _lastResults.Count,
            HealthyChecks = healthy,
            UnhealthyChecks = unhealthy,
            DegradedChecks = degraded,
            Checks = _lastResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private async void ExecuteHealthChecks(object? state)
    {
        try
        {
            var results = await ExecuteAllHealthChecks();
            var overallHealth = GetOverallHealth();
            
            _logger.LogInformation("🏥 Health checks executados: {Healthy}/{Total} saudáveis", 
                overallHealth.HealthyChecks, overallHealth.TotalChecks);

            if (overallHealth.Status == HealthStatus.Unhealthy)
            {
                _logger.LogWarning("⚠️ Sistema não saudável detectado");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar health checks");
        }
    }

    // Health Checks específicos
    private async Task<HealthCheckResult> CheckDatabaseHealth()
    {
        await Task.Delay(Random.Shared.Next(10, 100)); // Simula latência de BD
        
        var isHealthy = Random.Shared.NextDouble() > 0.05; // 95% chance de sucesso
        var connectionCount = Random.Shared.Next(1, 100);
        
        return new HealthCheckResult
        {
            Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            Description = isHealthy ? "Database connection successful" : "Database connection failed",
            Data = new Dictionary<string, object>
            {
                ["connection_count"] = connectionCount,
                ["database_name"] = "microservices_db",
                ["server"] = "localhost:5432"
            }
        };
    }

    private async Task<HealthCheckResult> CheckRedisHealth()
    {
        await Task.Delay(Random.Shared.Next(5, 50));
        
        var isHealthy = Random.Shared.NextDouble() > 0.03;
        var usedMemory = Random.Shared.Next(100, 1000);
        
        return new HealthCheckResult
        {
            Status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
            Description = isHealthy ? "Redis cache operational" : "Redis cache unreachable",
            Data = new Dictionary<string, object>
            {
                ["used_memory_mb"] = usedMemory,
                ["connected_clients"] = Random.Shared.Next(1, 50),
                ["server"] = "localhost:6379"
            }
        };
    }

    private async Task<HealthCheckResult> CheckMessageQueueHealth()
    {
        await Task.Delay(Random.Shared.Next(15, 80));
        
        var isHealthy = Random.Shared.NextDouble() > 0.04;
        var queueDepth = Random.Shared.Next(0, 1000);
        
        var status = queueDepth > 500 ? HealthStatus.Degraded : 
                    isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        
        return new HealthCheckResult
        {
            Status = status,
            Description = status switch
            {
                HealthStatus.Healthy => "Message queue operational",
                HealthStatus.Degraded => "Message queue has high depth",
                _ => "Message queue unavailable"
            },
            Data = new Dictionary<string, object>
            {
                ["queue_depth"] = queueDepth,
                ["consumers"] = Random.Shared.Next(1, 10),
                ["server"] = "localhost:5672"
            }
        };
    }

    private async Task<HealthCheckResult> CheckExternalApiHealth()
    {
        await Task.Delay(Random.Shared.Next(50, 200));
        
        var isHealthy = Random.Shared.NextDouble() > 0.1; // 90% chance de sucesso
        var responseTime = Random.Shared.Next(100, 2000);
        
        var status = responseTime > 1000 ? HealthStatus.Degraded :
                    isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        
        return new HealthCheckResult
        {
            Status = status,
            Description = status switch
            {
                HealthStatus.Healthy => "External API responsive",
                HealthStatus.Degraded => "External API slow response",
                _ => "External API unavailable"
            },
            Data = new Dictionary<string, object>
            {
                ["endpoint"] = "https://api.external-service.com",
                ["avg_response_time_ms"] = responseTime,
                ["last_success"] = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 60))
            }
        };
    }

    private async Task<HealthCheckResult> CheckDiskSpaceHealth()
    {
        await Task.Delay(Random.Shared.Next(5, 20));
        
        var usedSpacePercent = Random.Shared.Next(20, 95);
        var availableGB = Random.Shared.Next(5, 100);
        
        var status = usedSpacePercent > 90 ? HealthStatus.Unhealthy :
                    usedSpacePercent > 80 ? HealthStatus.Degraded :
                    HealthStatus.Healthy;
        
        return new HealthCheckResult
        {
            Status = status,
            Description = $"Disk usage at {usedSpacePercent}%",
            Data = new Dictionary<string, object>
            {
                ["used_space_percent"] = usedSpacePercent,
                ["available_gb"] = availableGB,
                ["mount_point"] = "/"
            }
        };
    }

    private async Task<HealthCheckResult> CheckMemoryHealth()
    {
        await Task.Delay(Random.Shared.Next(5, 15));
        
        var usedMemoryPercent = Random.Shared.Next(30, 95);
        var totalMemoryGB = 8;
        var usedMemoryGB = Math.Round(totalMemoryGB * usedMemoryPercent / 100.0, 1);
        
        var status = usedMemoryPercent > 90 ? HealthStatus.Unhealthy :
                    usedMemoryPercent > 80 ? HealthStatus.Degraded :
                    HealthStatus.Healthy;
        
        return new HealthCheckResult
        {
            Status = status,
            Description = $"Memory usage at {usedMemoryPercent}%",
            Data = new Dictionary<string, object>
            {
                ["used_memory_percent"] = usedMemoryPercent,
                ["used_memory_gb"] = usedMemoryGB,
                ["total_memory_gb"] = totalMemoryGB
            }
        };
    }

    public async Task DemonstrateHealthChecks()
    {
        Console.WriteLine("🏥 Health Check System Demonstration");
        Console.WriteLine("===================================");

        await DemonstrateIndividualHealthChecks();
        await DemonstrateOverallHealth();
        await DemonstrateHealthCheckMonitoring();
    }

    private async Task DemonstrateIndividualHealthChecks()
    {
        Console.WriteLine("\n💚 1. Individual Health Checks");
        Console.WriteLine("------------------------------");

        foreach (var healthCheckName in _healthChecks.Keys)
        {
            var result = await ExecuteHealthCheck(healthCheckName);
            var statusIcon = result.Status switch
            {
                HealthStatus.Healthy => "🟢",
                HealthStatus.Degraded => "🟡",
                HealthStatus.Unhealthy => "🔴",
                _ => "⚪"
            };

            Console.WriteLine($"{statusIcon} {healthCheckName}: {result.Description} " +
                $"({result.ResponseTime.TotalMilliseconds:F0}ms)");

            if (result.Data.Any())
            {
                var dataJson = JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = false });
                Console.WriteLine($"   📊 Data: {dataJson}");
            }
        }
    }

    private async Task DemonstrateOverallHealth()
    {
        Console.WriteLine("\n📊 2. Overall System Health");
        Console.WriteLine("--------------------------");

        var overallHealth = GetOverallHealth();
        var statusIcon = overallHealth.Status switch
        {
            HealthStatus.Healthy => "🟢",
            HealthStatus.Degraded => "🟡",
            HealthStatus.Unhealthy => "🔴",
            _ => "⚪"
        };

        Console.WriteLine($"{statusIcon} Overall Status: {overallHealth.Status}");
        Console.WriteLine($"📈 Health Summary:");
        Console.WriteLine($"   ✅ Healthy: {overallHealth.HealthyChecks}");
        Console.WriteLine($"   ⚠️  Degraded: {overallHealth.DegradedChecks}");
        Console.WriteLine($"   ❌ Unhealthy: {overallHealth.UnhealthyChecks}");
        Console.WriteLine($"   📊 Total: {overallHealth.TotalChecks}");

        await Task.CompletedTask;
    }

    private async Task DemonstrateHealthCheckMonitoring()
    {
        Console.WriteLine("\n📡 3. Continuous Health Monitoring");
        Console.WriteLine("---------------------------------");

        Console.WriteLine("🔄 Executando monitoramento por 10 segundos...");
        
        var startTime = DateTime.UtcNow;
        var healthHistory = new List<OverallHealthStatus>();
        
        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(10))
        {
            var currentHealth = GetOverallHealth();
            healthHistory.Add(currentHealth);
            
            var statusIcon = currentHealth.Status switch
            {
                HealthStatus.Healthy => "🟢",
                HealthStatus.Degraded => "🟡",
                HealthStatus.Unhealthy => "🔴",
                _ => "⚪"
            };
            
            Console.WriteLine($"   {DateTime.UtcNow:HH:mm:ss} {statusIcon} " +
                $"{currentHealth.HealthyChecks}/{currentHealth.TotalChecks} healthy");
            
            await Task.Delay(2000);
        }

        // Análise do histórico
        var healthyCount = healthHistory.Count(h => h.Status == HealthStatus.Healthy);
        var degradedCount = healthHistory.Count(h => h.Status == HealthStatus.Degraded);
        var unhealthyCount = healthHistory.Count(h => h.Status == HealthStatus.Unhealthy);

        Console.WriteLine($"\n📈 Health History Analysis:");
        Console.WriteLine($"   🟢 Healthy periods: {healthyCount}/{healthHistory.Count}");
        Console.WriteLine($"   🟡 Degraded periods: {degradedCount}/{healthHistory.Count}");
        Console.WriteLine($"   🔴 Unhealthy periods: {unhealthyCount}/{healthHistory.Count}");
        
        var uptime = Math.Round((double)healthyCount / healthHistory.Count * 100, 1);
        Console.WriteLine($"   ⏱️  Uptime: {uptime}%");
    }

    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
    }
}

public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class OverallHealthStatus
{
    public HealthStatus Status { get; set; }
    public int TotalChecks { get; set; }
    public int HealthyChecks { get; set; }
    public int UnhealthyChecks { get; set; }
    public int DegradedChecks { get; set; }
    public Dictionary<string, HealthCheckResult> Checks { get; set; } = new();
}

public enum HealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}
