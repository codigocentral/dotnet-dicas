using System.Collections.Concurrent;
using System.Threading.Channels;
using Dica70_BackgroundServices.Models;

namespace Dica70_BackgroundServices.Services;

/// <summary>
/// Serviço para gerenciar filas de processamento em memória
/// Demonstra padrões de producer/consumer com background services
/// </summary>
public interface IQueueService<T>
{
    Task EnqueueAsync(QueueItem<T> item);
    Task<QueueItem<T>?> DequeueAsync(CancellationToken cancellationToken = default);
    int Count { get; }
    bool IsEmpty { get; }
}

public class ChannelQueueService<T> : IQueueService<T>
{
    private readonly Channel<QueueItem<T>> _channel;
    private readonly ChannelWriter<QueueItem<T>> _writer;
    private readonly ChannelReader<QueueItem<T>> _reader;

    public ChannelQueueService(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<QueueItem<T>>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public async Task EnqueueAsync(QueueItem<T> item)
    {
        await _writer.WriteAsync(item);
    }

    public async Task<QueueItem<T>?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (await _reader.WaitToReadAsync(cancellationToken))
        {
            if (_reader.TryRead(out var item))
            {
                return item;
            }
        }
        return null;
    }

    public int Count => _reader.Count;
    public bool IsEmpty => _reader.Count == 0;

    public void Complete()
    {
        _writer.Complete();
    }
}

/// <summary>
/// Fila com prioridade usando ConcurrentPriorityQueue
/// </summary>
public class PriorityQueueService<T> : IQueueService<T>
{
    private readonly PriorityQueue<QueueItem<T>, int> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly object _lock = new();

    public Task EnqueueAsync(QueueItem<T> item)
    {
        lock (_lock)
        {
            // Prioridade negativa para que itens de maior prioridade sejam processados primeiro
            _queue.Enqueue(item, -item.Priority);
        }
        _semaphore.Release();
        return Task.CompletedTask;
    }

    public async Task<QueueItem<T>?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        lock (_lock)
        {
            if (_queue.TryDequeue(out var item, out _))
            {
                return item;
            }
        }
        
        return null;
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }

    public bool IsEmpty
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count == 0;
            }
        }
    }
}

/// <summary>
/// Serviço para rastreamento de jobs e status
/// </summary>
public interface IJobTrackingService
{
    Task<string> CreateJobAsync(string name, string description);
    Task UpdateJobStatusAsync(string jobId, JobStatus status, string? error = null);
    Task<JobInfo?> GetJobAsync(string jobId);
    Task<List<JobInfo>> GetJobsAsync(JobStatus? status = null, int limit = 50);
    Task<ServiceStatus> GetServiceStatusAsync(string serviceName);
    Task UpdateServiceMetricsAsync(string serviceName, long processedItems, long errorCount);
}

public class JobTrackingService : IJobTrackingService
{
    private readonly ConcurrentDictionary<string, JobInfo> _jobs = new();
    private readonly ConcurrentDictionary<string, ServiceStatus> _serviceStatuses = new();

    public Task<string> CreateJobAsync(string name, string description)
    {
        var job = new JobInfo
        {
            Name = name,
            Description = description,
            Status = JobStatus.Pending
        };

        _jobs[job.Id] = job;
        return Task.FromResult(job.Id);
    }

    public Task UpdateJobStatusAsync(string jobId, JobStatus status, string? error = null)
    {
        if (_jobs.TryGetValue(jobId, out var existingJob))
        {
            var updatedJob = existingJob with
            {
                Status = status,
                StartedAt = status == JobStatus.Running && existingJob.StartedAt == null 
                    ? DateTime.UtcNow : existingJob.StartedAt,
                CompletedAt = status is JobStatus.Completed or JobStatus.Failed or JobStatus.Cancelled 
                    ? DateTime.UtcNow : existingJob.CompletedAt,
                Error = error
            };

            _jobs[jobId] = updatedJob;
        }

        return Task.CompletedTask;
    }

    public Task<JobInfo?> GetJobAsync(string jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return Task.FromResult(job);
    }

    public Task<List<JobInfo>> GetJobsAsync(JobStatus? status = null, int limit = 50)
    {
        var query = _jobs.Values.AsEnumerable();
        
        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        var jobs = query
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult(jobs);
    }

    public Task<ServiceStatus> GetServiceStatusAsync(string serviceName)
    {
        if (_serviceStatuses.TryGetValue(serviceName, out var status))
        {
            return Task.FromResult(status);
        }

        var defaultStatus = new ServiceStatus
        {
            ServiceName = serviceName,
            IsRunning = false,
            LastActivity = DateTime.UtcNow
        };

        return Task.FromResult(defaultStatus);
    }

    public Task UpdateServiceMetricsAsync(string serviceName, long processedItems, long errorCount)
    {
        _serviceStatuses.AddOrUpdate(serviceName,
            new ServiceStatus
            {
                ServiceName = serviceName,
                IsRunning = true,
                LastActivity = DateTime.UtcNow,
                ProcessedItems = processedItems,
                ErrorCount = errorCount,
                Uptime = TimeSpan.Zero
            },
            (key, existing) => existing with
            {
                IsRunning = true,
                LastActivity = DateTime.UtcNow,
                ProcessedItems = processedItems,
                ErrorCount = errorCount,
                Uptime = DateTime.UtcNow - existing.LastActivity + existing.Uptime
            });

        return Task.CompletedTask;
    }
}
