using Microsoft.AspNetCore.Mvc;
using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;

namespace Dica70_BackgroundServices.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QueueController : ControllerBase
{
    private readonly IQueueService<EmailNotification> _emailQueue;
    private readonly IQueueService<DataProcessingJob> _dataQueue;
    private readonly IJobTrackingService _jobTracking;
    private readonly ILogger<QueueController> _logger;

    public QueueController(
        IQueueService<EmailNotification> emailQueue,
        IQueueService<DataProcessingJob> dataQueue,
        IJobTrackingService jobTracking,
        ILogger<QueueController> logger)
    {
        _emailQueue = emailQueue;
        _dataQueue = dataQueue;
        _jobTracking = jobTracking;
        _logger = logger;
    }

    /// <summary>
    /// Adiciona um email à fila de processamento
    /// </summary>
    [HttpPost("email")]
    public async Task<ActionResult<ApiResponse<string>>> QueueEmail([FromBody] EmailNotification email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email.To) || string.IsNullOrWhiteSpace(email.Subject))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Email deve conter destinatário e assunto"
                });
            }

            var queueItem = new QueueItem<EmailNotification>
            {
                Data = email,
                Priority = (int)email.Priority
            };

            await _emailQueue.EnqueueAsync(queueItem);

            _logger.LogInformation("Email adicionado à fila: {To} - Prioridade: {Priority}", 
                email.To, email.Priority);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = queueItem.Id,
                Message = "Email adicionado à fila com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar email à fila");
            
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Adiciona um job de processamento de dados à fila
    /// </summary>
    [HttpPost("data-processing")]
    public async Task<ActionResult<ApiResponse<string>>> QueueDataProcessing([FromBody] DataProcessingJob job)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(job.Source) || string.IsNullOrWhiteSpace(job.Destination))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Job deve conter origem e destino"
                });
            }

            if (job.RecordCount <= 0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Número de registros deve ser maior que zero"
                });
            }

            var queueItem = new QueueItem<DataProcessingJob>
            {
                Data = job,
                Priority = job.RecordCount > 10000 ? 1 : 0 // Prioridade alta para jobs grandes
            };

            await _dataQueue.EnqueueAsync(queueItem);

            _logger.LogInformation("Job de processamento adicionado à fila: {Source} -> {Destination} ({RecordCount} registros)",
                job.Source, job.Destination, job.RecordCount);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Data = queueItem.Id,
                Message = "Job adicionado à fila com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar job à fila");
            
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Obtém estatísticas das filas
    /// </summary>
    [HttpGet("status")]
    public ActionResult<ApiResponse<object>> GetQueueStatus()
    {
        try
        {
            var status = new
            {
                email_queue = new
                {
                    count = _emailQueue.Count,
                    is_empty = _emailQueue.IsEmpty
                },
                data_processing_queue = new
                {
                    count = _dataQueue.Count,
                    is_empty = _dataQueue.IsEmpty
                },
                timestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = status,
                Message = "Status das filas obtido com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status das filas");
            
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Adiciona vários emails de teste à fila
    /// </summary>
    [HttpPost("email/test-batch")]
    public async Task<ActionResult<ApiResponse<List<string>>>> QueueTestEmails([FromQuery] int count = 10)
    {
        try
        {
            if (count > 100) count = 100; // Limita para evitar sobrecarga

            var queueIds = new List<string>();
            var priorities = Enum.GetValues<EmailPriority>();

            for (int i = 0; i < count; i++)
            {
                var email = new EmailNotification
                {
                    To = $"user{i + 1}@exemplo.com",
                    Subject = $"Email de Teste {i + 1}",
                    Body = $"Este é um email de teste número {i + 1} gerado automaticamente.",
                    Priority = priorities[Random.Shared.Next(priorities.Length)]
                };

                var queueItem = new QueueItem<EmailNotification>
                {
                    Data = email,
                    Priority = (int)email.Priority
                };

                await _emailQueue.EnqueueAsync(queueItem);
                queueIds.Add(queueItem.Id);
            }

            _logger.LogInformation("Adicionados {Count} emails de teste à fila", count);

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = queueIds,
                Message = $"{count} emails de teste adicionados à fila"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar emails de teste");
            
            return StatusCode(500, new ApiResponse<List<string>>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    /// <summary>
    /// Adiciona vários jobs de processamento de teste à fila
    /// </summary>
    [HttpPost("data-processing/test-batch")]
    public async Task<ActionResult<ApiResponse<List<string>>>> QueueTestDataJobs([FromQuery] int count = 5)
    {
        try
        {
            if (count > 50) count = 50; // Limita para evitar sobrecarga

            var queueIds = new List<string>();
            var processingTypes = new[] { "ETL", "Analytics", "Migration", "Backup", "Cleanup" };

            for (int i = 0; i < count; i++)
            {
                var job = new DataProcessingJob
                {
                    Source = $"database_table_{i + 1}",
                    Destination = $"data_warehouse_table_{i + 1}",
                    ProcessingType = processingTypes[Random.Shared.Next(processingTypes.Length)],
                    RecordCount = Random.Shared.Next(1000, 50000),
                    Parameters = new Dictionary<string, object>
                    {
                        ["batch_size"] = Random.Shared.Next(100, 1000),
                        ["timeout_minutes"] = Random.Shared.Next(30, 120)
                    }
                };

                var queueItem = new QueueItem<DataProcessingJob>
                {
                    Data = job,
                    Priority = job.RecordCount > 10000 ? 1 : 0
                };

                await _dataQueue.EnqueueAsync(queueItem);
                queueIds.Add(queueItem.Id);
            }

            _logger.LogInformation("Adicionados {Count} jobs de processamento de teste à fila", count);

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = queueIds,
                Message = $"{count} jobs de processamento adicionados à fila"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jobs de teste");
            
            return StatusCode(500, new ApiResponse<List<string>>
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}
