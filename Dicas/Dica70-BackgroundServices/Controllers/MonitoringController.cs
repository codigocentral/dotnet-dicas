using Microsoft.AspNetCore.Mvc;
using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;
using Dica70_BackgroundServices.BackgroundServices;

namespace Dica70_BackgroundServices.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MonitoringController : ControllerBase
{
    private readonly IJobTrackingService _jobTracking;
    private readonly HealthMonitoringService _healthMonitoring;
    private readonly SingletonBackgroundService _singletonService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IJobTrackingService jobTracking,
        HealthMonitoringService healthMonitoring,
        SingletonBackgroundService singletonService,
        ILogger<MonitoringController> logger)
    {
        _jobTracking = jobTracking;
        _healthMonitoring = healthMonitoring;
        _singletonService = singletonService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o status de todos os serviços de background
    /// </summary>
    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<List<ServiceStatus>>>> GetServicesStatus()
    {
        try
        {
            var serviceNames = new[]
            {
                "TimedBackgroundService",
                "EmailBackgroundService",
                "DataProcessingBackgroundService",
                "HealthMonitoringService",
                "SingletonBackgroundService"
            };

            var statuses = new List<ServiceStatus>();

            foreach (var serviceName in serviceNames)
            {
                var status = await _jobTracking.GetServiceStatusAsync(serviceName);
                statuses.Add(status);
            }

            return Ok(new ApiResponse<List<ServiceStatus>>
            {
                Success = true,
                Data = statuses,
                Message = "Status dos serviços obtido com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status dos serviços");
            
            return StatusCode(500, new ApiResponse<List<ServiceStatus>>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém o status de um serviço específico
    /// </summary>
    [HttpGet("services/{serviceName}")]
    public async Task<ActionResult<ApiResponse<ServiceStatus>>> GetServiceStatus(string serviceName)
    {
        try
        {
            var status = await _jobTracking.GetServiceStatusAsync(serviceName);

            return Ok(new ApiResponse<ServiceStatus>
            {
                Success = true,
                Data = status,
                Message = $"Status do serviço {serviceName} obtido com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do serviço {ServiceName}", serviceName);
            
            return StatusCode(500, new ApiResponse<ServiceStatus>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém os resultados recentes do health check
    /// </summary>
    [HttpGet("health")]
    public ActionResult<ApiResponse<List<HealthCheckResult>>> GetHealthChecks([FromQuery] int count = 10)
    {
        try
        {
            var healthChecks = _healthMonitoring.GetRecentHealthChecks(count);

            return Ok(new ApiResponse<List<HealthCheckResult>>
            {
                Success = true,
                Data = healthChecks,
                Message = $"Últimos {healthChecks.Count} health checks obtidos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter health checks");
            
            return StatusCode(500, new ApiResponse<List<HealthCheckResult>>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém estatísticas agregadas do health monitoring
    /// </summary>
    [HttpGet("health/statistics")]
    public ActionResult<ApiResponse<object>> GetHealthStatistics()
    {
        try
        {
            var healthChecks = _healthMonitoring.GetRecentHealthChecks(100);
            
            if (!healthChecks.Any())
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { message = "Nenhum health check disponível ainda" },
                    Message = "Aguardando primeiros health checks"
                });
            }

            var groupedByService = healthChecks.GroupBy(h => h.ServiceName);
            
            var statistics = new
            {
                overall_health = healthChecks.Count(h => h.IsHealthy) / (double)healthChecks.Count * 100,
                total_checks = healthChecks.Count,
                services = groupedByService.Select(g => new
                {
                    service_name = g.Key,
                    health_percentage = g.Count(h => h.IsHealthy) / (double)g.Count() * 100,
                    avg_response_time_ms = g.Average(h => h.ResponseTime.TotalMilliseconds),
                    last_check = g.Max(h => h.CheckedAt),
                    total_checks = g.Count()
                }).ToList(),
                last_update = healthChecks.Max(h => h.CheckedAt)
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = statistics,
                Message = "Estatísticas de saúde calculadas com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular estatísticas de saúde");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém informações sobre o singleton service
    /// </summary>
    [HttpGet("singleton/status")]
    public ActionResult<ApiResponse<object>> GetSingletonStatus()
    {
        try
        {
            var status = new
            {
                is_processing = _singletonService.IsProcessing,
                timestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = status,
                Message = "Status do singleton service obtido"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do singleton service");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém dashboard com visão geral do sistema
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
    {
        try
        {
            // Obter status dos serviços
            var serviceNames = new[]
            {
                "TimedBackgroundService",
                "EmailBackgroundService", 
                "DataProcessingBackgroundService",
                "HealthMonitoringService",
                "SingletonBackgroundService"
            };

            var serviceStatuses = new List<ServiceStatus>();
            foreach (var serviceName in serviceNames)
            {
                var status = await _jobTracking.GetServiceStatusAsync(serviceName);
                serviceStatuses.Add(status);
            }

            // Obter jobs recentes
            var recentJobs = await _jobTracking.GetJobsAsync(limit: 20);
            
            // Obter health checks recentes
            var healthChecks = _healthMonitoring.GetRecentHealthChecks(10);

            var dashboard = new
            {
                services = new
                {
                    total = serviceStatuses.Count,
                    running = serviceStatuses.Count(s => s.IsRunning),
                    details = serviceStatuses.Select(s => new
                    {
                        name = s.ServiceName,
                        is_running = s.IsRunning,
                        processed_items = s.ProcessedItems,
                        error_count = s.ErrorCount,
                        last_activity = s.LastActivity
                    })
                },
                jobs = new
                {
                    total_recent = recentJobs.Count,
                    by_status = recentJobs.GroupBy(j => j.Status)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    success_rate = recentJobs.Count > 0 
                        ? recentJobs.Count(j => j.Status == JobStatus.Completed) / (double)recentJobs.Count * 100
                        : 0
                },
                health = new
                {
                    total_checks = healthChecks.Count,
                    healthy_percentage = healthChecks.Count > 0 
                        ? healthChecks.Count(h => h.IsHealthy) / (double)healthChecks.Count * 100
                        : 0,
                    last_check = healthChecks.FirstOrDefault()?.CheckedAt
                },
                singleton_service = new
                {
                    is_processing = _singletonService.IsProcessing
                },
                timestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = dashboard,
                Message = "Dashboard obtido com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dashboard");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}
