using Microsoft.AspNetCore.Mvc;
using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;

namespace Dica70_BackgroundServices.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobTrackingService _jobTracking;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobTrackingService jobTracking, ILogger<JobsController> logger)
    {
        _jobTracking = jobTracking;
        _logger = logger;
    }

    /// <summary>
    /// Obtém informações de um job específico
    /// </summary>
    [HttpGet("{jobId}")]
    public async Task<ActionResult<ApiResponse<JobInfo>>> GetJob(string jobId)
    {
        try
        {
            var job = await _jobTracking.GetJobAsync(jobId);
            
            if (job == null)
            {
                return NotFound(new ApiResponse<JobInfo>
                {
                    Success = false,
                    Message = "Job não encontrado"
                });
            }

            return Ok(new ApiResponse<JobInfo>
            {
                Success = true,
                Data = job,
                Message = "Job encontrado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar job {JobId}", jobId);
            
            return StatusCode(500, new ApiResponse<JobInfo>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Lista jobs com filtros opcionais
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobInfo>>>> GetJobs(
        [FromQuery] JobStatus? status = null,
        [FromQuery] int limit = 50)
    {
        try
        {
            if (limit > 100) limit = 100; // Limita para evitar sobrecarga

            var jobs = await _jobTracking.GetJobsAsync(status, limit);

            return Ok(new ApiResponse<List<JobInfo>>
            {
                Success = true,
                Data = jobs,
                Message = $"Encontrados {jobs.Count} jobs",
                Metadata = new Dictionary<string, object>
                {
                    ["total"] = jobs.Count,
                    ["status_filter"] = status?.ToString() ?? "all",
                    ["limit"] = limit
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar jobs");
            
            return StatusCode(500, new ApiResponse<List<JobInfo>>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém estatísticas dos jobs
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<object>>> GetJobStatistics()
    {
        try
        {
            var allJobs = await _jobTracking.GetJobsAsync(limit: 1000);
            
            var statistics = new
            {
                total = allJobs.Count,
                by_status = allJobs.GroupBy(j => j.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                avg_duration_seconds = allJobs
                    .Where(j => j.Duration.HasValue)
                    .Select(j => j.Duration!.Value.TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average(),
                recent_activity = allJobs
                    .Where(j => j.CreatedAt > DateTime.UtcNow.AddHours(-24))
                    .Count(),
                success_rate = allJobs.Count > 0 
                    ? (double)allJobs.Count(j => j.Status == JobStatus.Completed) / allJobs.Count * 100
                    : 0
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = statistics,
                Message = "Estatísticas calculadas com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular estatísticas dos jobs");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}
