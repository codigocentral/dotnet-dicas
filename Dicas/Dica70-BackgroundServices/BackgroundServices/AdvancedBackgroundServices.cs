using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;

namespace Dica70_BackgroundServices.BackgroundServices;

/// <summary>
/// Serviço de background para monitoramento de saúde do sistema
/// Demonstra padrão de health monitoring
/// </summary>
public class HealthMonitoringService : BackgroundService
{
    private readonly ILogger<HealthMonitoringService> _logger;
    private readonly IJobTrackingService _jobTracking;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<HealthCheckResult> _healthHistory = new();
    private readonly object _lock = new();

    public HealthMonitoringService(
        ILogger<HealthMonitoringService> logger,
        IJobTrackingService jobTracking,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _jobTracking = jobTracking;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health Monitoring Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthChecksAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Health Monitoring Service foi cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Health Monitoring Service");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
    {
        var checks = new[]
        {
            CheckDatabaseHealthAsync(cancellationToken),
            CheckExternalApiHealthAsync(cancellationToken),
            CheckMemoryUsageAsync(cancellationToken),
            CheckDiskSpaceAsync(cancellationToken)
        };

        var results = await Task.WhenAll(checks);

        lock (_lock)
        {
            _healthHistory.AddRange(results);
            
            // Manter apenas os últimos 100 resultados
            while (_healthHistory.Count > 100)
            {
                _healthHistory.RemoveAt(0);
            }
        }

        var unhealthyServices = results.Where(r => !r.IsHealthy).ToList();
        
        if (unhealthyServices.Any())
        {
            _logger.LogWarning("Serviços com problemas detectados: {Services}",
                string.Join(", ", unhealthyServices.Select(s => s.ServiceName)));
            
            // Aqui poderia enviar alertas, notificações, etc.
        }
        else
        {
            _logger.LogDebug("Todos os serviços estão saudáveis");
        }

        await _jobTracking.UpdateServiceMetricsAsync("HealthMonitoringService", results.Length, unhealthyServices.Count);
    }

    private async Task<HealthCheckResult> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Simula verificação de banco de dados
            await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
            
            var isHealthy = Random.Shared.NextDouble() > 0.05; // 95% de chance de estar saudável
            
            return new HealthCheckResult
            {
                ServiceName = "Database",
                IsHealthy = isHealthy,
                ResponseTime = stopwatch.Elapsed,
                Details = isHealthy ? "Conexão estabelecida com sucesso" : "Timeout na conexão"
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                ServiceName = "Database",
                IsHealthy = false,
                ResponseTime = stopwatch.Elapsed,
                Details = ex.Message
            };
        }
    }

    private async Task<HealthCheckResult> CheckExternalApiHealthAsync(CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Simula verificação de API externa
            await Task.Delay(Random.Shared.Next(200, 800), cancellationToken);
            
            var isHealthy = Random.Shared.NextDouble() > 0.1; // 90% de chance de estar saudável
            
            return new HealthCheckResult
            {
                ServiceName = "External API",
                IsHealthy = isHealthy,
                ResponseTime = stopwatch.Elapsed,
                Details = isHealthy ? "API respondendo normalmente" : "API indisponível"
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                ServiceName = "External API",
                IsHealthy = false,
                ResponseTime = stopwatch.Elapsed,
                Details = ex.Message
            };
        }
    }

    private async Task<HealthCheckResult> CheckMemoryUsageAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / 1024 / 1024;
        var isHealthy = memoryMB < 500; // Considera saudável se usar menos de 500MB
        
        return new HealthCheckResult
        {
            ServiceName = "Memory Usage",
            IsHealthy = isHealthy,
            ResponseTime = TimeSpan.FromMilliseconds(50),
            Details = $"Uso atual: {memoryMB}MB"
        };
    }

    private async Task<HealthCheckResult> CheckDiskSpaceAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(30, cancellationToken);
        
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            var hasLowSpace = drives.Any(d => d.AvailableFreeSpace < d.TotalSize * 0.1); // Menos de 10% livre
            
            return new HealthCheckResult
            {
                ServiceName = "Disk Space",
                IsHealthy = !hasLowSpace,
                ResponseTime = TimeSpan.FromMilliseconds(30),
                Details = hasLowSpace ? "Espaço em disco baixo" : "Espaço em disco suficiente"
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult
            {
                ServiceName = "Disk Space",
                IsHealthy = false,
                ResponseTime = TimeSpan.FromMilliseconds(30),
                Details = ex.Message
            };
        }
    }

    public List<HealthCheckResult> GetRecentHealthChecks(int count = 10)
    {
        lock (_lock)
        {
            return _healthHistory
                .OrderByDescending(h => h.CheckedAt)
                .Take(count)
                .ToList();
        }
    }
}

/// <summary>
/// Serviço de background com padrão de consumidor singleton
/// Demonstra como implementar um singleton background service
/// </summary>
public class SingletonBackgroundService : BackgroundService
{
    private readonly ILogger<SingletonBackgroundService> _logger;
    private readonly IJobTrackingService _jobTracking;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _isProcessing = false;

    public SingletonBackgroundService(
        ILogger<SingletonBackgroundService> logger,
        IJobTrackingService jobTracking)
    {
        _logger = logger;
        _jobTracking = jobTracking;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Singleton Background Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            await _semaphore.WaitAsync(stoppingToken);
            
            try
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    await ProcessCriticalTaskAsync(stoppingToken);
                    _isProcessing = false;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Singleton Background Service foi cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Singleton Background Service");
                _isProcessing = false;
            }
            finally
            {
                _semaphore.Release();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

    private async Task ProcessCriticalTaskAsync(CancellationToken cancellationToken)
    {
        var jobId = await _jobTracking.CreateJobAsync(
            "Tarefa Crítica Singleton",
            "Processamento que deve executar apenas uma vez por instância");

        try
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Running);

            _logger.LogInformation("Executando tarefa crítica singleton às {Time}", DateTime.UtcNow);

            // Simula processamento crítico que não pode ser executado em paralelo
            for (int i = 0; i < 5; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                _logger.LogDebug("Processando etapa {Step}/5 da tarefa crítica", i + 1);
                await Task.Delay(2000, cancellationToken);
            }

            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Completed);
            await _jobTracking.UpdateServiceMetricsAsync("SingletonBackgroundService", 1, 0);

            _logger.LogInformation("Tarefa crítica singleton concluída com sucesso");
        }
        catch (Exception ex)
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Failed, ex.Message);
            await _jobTracking.UpdateServiceMetricsAsync("SingletonBackgroundService", 0, 1);
            throw;
        }
    }

    public bool IsProcessing => _isProcessing;
}
