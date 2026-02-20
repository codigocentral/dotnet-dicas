using Dica70_BackgroundServices.Models;
using Dica70_BackgroundServices.Services;

namespace Dica70_BackgroundServices.BackgroundServices;

/// <summary>
/// Serviço de background que executa tarefas agendadas
/// Demonstra padrão de Timer-based background service
/// </summary>
public class TimedBackgroundService : BackgroundService
{
    private readonly ILogger<TimedBackgroundService> _logger;
    private readonly IJobTrackingService _jobTracking;
    private readonly IServiceScope _scope;

    public TimedBackgroundService(
        ILogger<TimedBackgroundService> logger,
        IJobTrackingService jobTracking,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _jobTracking = jobTracking;
        _scope = scopeFactory.CreateScope();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Background Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Background Service foi cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Timed Background Service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        var jobId = await _jobTracking.CreateJobAsync(
            "Limpeza Automática",
            "Limpeza de dados temporários e logs antigos");

        try
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Running);
            
            _logger.LogInformation("Executando limpeza automática às {Time}", DateTime.UtcNow);

            // Simula limpeza de dados
            await SimulateDataCleanupAsync(cancellationToken);
            
            // Simula limpeza de logs
            await SimulateLogCleanupAsync(cancellationToken);

            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Completed);
            await _jobTracking.UpdateServiceMetricsAsync("TimedBackgroundService", 1, 0);
            
            _logger.LogInformation("Limpeza automática concluída com sucesso");
        }
        catch (Exception ex)
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Failed, ex.Message);
            await _jobTracking.UpdateServiceMetricsAsync("TimedBackgroundService", 0, 1);
            throw;
        }
    }

    private async Task SimulateDataCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removendo dados temporários...");
        await Task.Delay(2000, cancellationToken);
        _logger.LogInformation("Dados temporários removidos: {Count} registros", Random.Shared.Next(10, 100));
    }

    private async Task SimulateLogCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removendo logs antigos...");
        await Task.Delay(1500, cancellationToken);
        _logger.LogInformation("Logs antigos removidos: {Size}MB", Random.Shared.Next(50, 500));
    }

    public override void Dispose()
    {
        _scope?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Serviço de background que processa filas de email
/// Demonstra padrão de queue-based background service
/// </summary>
public class EmailBackgroundService : BackgroundService
{
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly IQueueService<EmailNotification> _emailQueue;
    private readonly IJobTrackingService _jobTracking;
    private long _processedCount = 0;
    private long _errorCount = 0;

    public EmailBackgroundService(
        ILogger<EmailBackgroundService> logger,
        IQueueService<EmailNotification> emailQueue,
        IJobTrackingService jobTracking)
    {
        _logger = logger;
        _emailQueue = emailQueue;
        _jobTracking = jobTracking;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Background Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var queueItem = await _emailQueue.DequeueAsync(stoppingToken);
                
                if (queueItem != null)
                {
                    await ProcessEmailAsync(queueItem, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Email Background Service foi cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Email Background Service");
                Interlocked.Increment(ref _errorCount);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task ProcessEmailAsync(QueueItem<EmailNotification> queueItem, CancellationToken cancellationToken)
    {
        var jobId = await _jobTracking.CreateJobAsync(
            "Envio de Email",
            $"Enviando email para {queueItem.Data.To}");

        try
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Running);

            _logger.LogInformation("Processando email para {To} - Prioridade: {Priority}",
                queueItem.Data.To, queueItem.Data.Priority);

            // Simula processamento de email baseado na prioridade
            var processingTime = queueItem.Data.Priority switch
            {
                EmailPriority.Critical => 500,
                EmailPriority.High => 1000,
                EmailPriority.Normal => 2000,
                EmailPriority.Low => 3000,
                _ => 2000
            };

            await Task.Delay(processingTime, cancellationToken);

            // Simula falha ocasional
            if (Random.Shared.NextDouble() < 0.1 && queueItem.RetryCount < queueItem.MaxRetries)
            {
                throw new InvalidOperationException("Falha temporária no servidor de email");
            }

            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Completed);
            Interlocked.Increment(ref _processedCount);

            await _jobTracking.UpdateServiceMetricsAsync("EmailBackgroundService", _processedCount, _errorCount);

            _logger.LogInformation("Email enviado com sucesso para {To}", queueItem.Data.To);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _errorCount);
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Failed, ex.Message);
            await _jobTracking.UpdateServiceMetricsAsync("EmailBackgroundService", _processedCount, _errorCount);

            _logger.LogWarning(ex, "Falha ao enviar email para {To}. Tentativa {Retry}/{MaxRetries}",
                queueItem.Data.To, queueItem.RetryCount + 1, queueItem.MaxRetries);

            // Requeue com retry se não excedeu o limite
            if (queueItem.RetryCount < queueItem.MaxRetries)
            {
                var retryItem = queueItem with
                {
                    RetryCount = queueItem.RetryCount + 1,
                    Error = ex.Message
                };

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryItem.RetryCount)), cancellationToken);
                await _emailQueue.EnqueueAsync(retryItem);
            }
        }
    }
}

/// <summary>
/// Serviço de background para processamento de dados em lote
/// Demonstra padrão de batch processing
/// </summary>
public class DataProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<DataProcessingBackgroundService> _logger;
    private readonly IQueueService<DataProcessingJob> _dataQueue;
    private readonly IJobTrackingService _jobTracking;
    private long _processedRecords = 0;
    private long _errorCount = 0;

    public DataProcessingBackgroundService(
        ILogger<DataProcessingBackgroundService> logger,
        IQueueService<DataProcessingJob> dataQueue,
        IJobTrackingService jobTracking)
    {
        _logger = logger;
        _dataQueue = dataQueue;
        _jobTracking = jobTracking;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Processing Background Service iniciado");

        // Processa até 3 jobs em paralelo
        var semaphore = new SemaphoreSlim(3, 3);
        var tasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await semaphore.WaitAsync(stoppingToken);

                var task = ProcessNextJobAsync(semaphore, stoppingToken);
                tasks.Add(task);

                // Remove tasks completas
                tasks.RemoveAll(t => t.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Data Processing Background Service foi cancelado");
                break;
            }
        }

        // Aguarda todas as tasks completarem
        await Task.WhenAll(tasks);
    }

    private async Task ProcessNextJobAsync(SemaphoreSlim semaphore, CancellationToken stoppingToken)
    {
        try
        {
            var queueItem = await _dataQueue.DequeueAsync(stoppingToken);
            
            if (queueItem != null)
            {
                await ProcessDataJobAsync(queueItem, stoppingToken);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ProcessDataJobAsync(QueueItem<DataProcessingJob> queueItem, CancellationToken cancellationToken)
    {
        var jobId = await _jobTracking.CreateJobAsync(
            "Processamento de Dados",
            $"Processando {queueItem.Data.RecordCount} registros de {queueItem.Data.Source}");

        try
        {
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Running);

            _logger.LogInformation("Iniciando processamento: {Source} -> {Destination} ({RecordCount} registros)",
                queueItem.Data.Source, queueItem.Data.Destination, queueItem.Data.RecordCount);

            // Simula processamento em lotes
            var batchSize = 100;
            var totalBatches = (queueItem.Data.RecordCount + batchSize - 1) / batchSize;

            for (int batch = 0; batch < totalBatches; batch++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var recordsInBatch = Math.Min(batchSize, queueItem.Data.RecordCount - (batch * batchSize));
                
                _logger.LogDebug("Processando lote {Batch}/{TotalBatches} ({RecordsInBatch} registros)",
                    batch + 1, totalBatches, recordsInBatch);

                // Simula processamento do lote
                await Task.Delay(500, cancellationToken);

                Interlocked.Add(ref _processedRecords, recordsInBatch);
            }

            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Completed);
            await _jobTracking.UpdateServiceMetricsAsync("DataProcessingBackgroundService", _processedRecords, _errorCount);

            _logger.LogInformation("Processamento concluído: {RecordCount} registros processados",
                queueItem.Data.RecordCount);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _errorCount);
            await _jobTracking.UpdateJobStatusAsync(jobId, JobStatus.Failed, ex.Message);
            await _jobTracking.UpdateServiceMetricsAsync("DataProcessingBackgroundService", _processedRecords, _errorCount);

            _logger.LogError(ex, "Erro no processamento de dados para job {Source} -> {Destination}",
                queueItem.Data.Source, queueItem.Data.Destination);
        }
    }
}
