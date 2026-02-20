using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;

namespace Dica26.Benchmark;

/// <summary>
/// Benchmark para demonstrar diferenças de performance entre
/// várias práticas de async/await
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Dica 26: Async/Await Best Practices Benchmarks ===\n");
        
        var config = DefaultConfig.Instance;
            
        BenchmarkRunner.Run<AsyncAwaitBenchmarks>(config);
    }
}

[MemoryDiagnoser]
public class AsyncAwaitBenchmarks
{
    private readonly Dictionary<string, string> _cache;
    private readonly string[] _keys;
    private readonly Random _random;

    public AsyncAwaitBenchmarks()
    {
        _cache = new Dictionary<string, string>();
        _random = new Random(42);
        
        // Pré-popula cache com alguns valores
        _keys = new[] { "key1", "key2", "key3", "key4", "key5" };
        foreach (var key in _keys)
        {
            _cache[key] = $"Cached value for {key}";
        }
    }

    /// <summary>
    /// Benchmark: Sequential vs Parallel execution
    /// </summary>
    [Benchmark(Baseline = true)]
    public async Task SequentialAwaits()
    {
        await SimulateWork(50);
        await SimulateWork(50);
        await SimulateWork(50);
    }

    [Benchmark]
    public async Task ParallelWithTaskWhenAll()
    {
        var tasks = new[]
        {
            SimulateWork(50),
            SimulateWork(50),
            SimulateWork(50)
        };
        
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task ParallelWithTaskWhenAllConfigureAwait()
    {
        var tasks = new[]
        {
            SimulateWorkWithConfigureAwait(50),
            SimulateWorkWithConfigureAwait(50),
            SimulateWorkWithConfigureAwait(50)
        };
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmark: Task vs ValueTask for cached operations
    /// </summary>
    [Benchmark]
    public async Task TaskBasedCaching()
    {
        for (int i = 0; i < 100; i++)
        {
            var key = _keys[i % _keys.Length];
            await GetDataWithTaskAsync(key);
        }
    }

    [Benchmark]
    public async Task ValueTaskBasedCaching()
    {
        for (int i = 0; i < 100; i++)
        {
            var key = _keys[i % _keys.Length];
            await GetDataWithValueTaskAsync(key);
        }
    }

    /// <summary>
    /// Benchmark: ConfigureAwait impact
    /// </summary>
    [Benchmark]
    public async Task WithoutConfigureAwait()
    {
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(1);
        }
    }

    [Benchmark]
    public async Task WithConfigureAwait()
    {
        for (int i = 0; i < 20; i++)
        {
            await Task.Delay(1).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Benchmark: Exception handling performance
    /// </summary>
    [Benchmark]
    public async Task NormalAsyncFlow()
    {
        for (int i = 0; i < 50; i++)
        {
            await SimulateWork(1);
        }
    }

    [Benchmark]
    public async Task AsyncFlowWithExceptionHandling()
    {
        for (int i = 0; i < 50; i++)
        {
            try
            {
                await SimulateWork(1);
            }
            catch (Exception)
            {
                // Handle exception (won't happen in this case)
            }
        }
    }

    [Benchmark]
    public async Task AsyncFlowWithCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        
        for (int i = 0; i < 50; i++)
        {
            await SimulateWorkWithCancellation(1, cts.Token);
        }
    }

    /// <summary>
    /// Benchmark: Allocation comparison between different async patterns
    /// </summary>
    [Benchmark]
    public async Task<string[]> TaskFromResultAllocation()
    {
        var results = new string[10];
        
        for (int i = 0; i < 10; i++)
        {
            results[i] = await Task.FromResult($"Result {i}");
        }
        
        return results;
    }

    [Benchmark]
    public async Task<string[]> ValueTaskFromResultAllocation()
    {
        var results = new string[10];
        
        for (int i = 0; i < 10; i++)
        {
            results[i] = await ValueTask.FromResult($"Result {i}");
        }
        
        return results;
    }

    [Benchmark]
    public string[] SynchronousCompletion()
    {
        var results = new string[10];
        
        for (int i = 0; i < 10; i++)
        {
            results[i] = $"Result {i}";
        }
        
        return results;
    }

    /// <summary>
    /// Benchmark: Different Task creation patterns
    /// </summary>
    [Benchmark]
    public async Task TaskRun()
    {
        await Task.Run(async () =>
        {
            await Task.Delay(10);
            return "TaskRun result";
        });
    }

    [Benchmark]
    public async Task TaskFactory()
    {
        await Task.Factory.StartNew(async () =>
        {
            await Task.Delay(10);
            return "TaskFactory result";
        }).Unwrap();
    }

    [Benchmark]
    public async Task DirectTaskDelay()
    {
        await Task.Delay(10);
    }

    // Helper methods
    private async Task SimulateWork(int delayMs)
    {
        await Task.Delay(delayMs);
    }

    private async Task SimulateWorkWithConfigureAwait(int delayMs)
    {
        await Task.Delay(delayMs).ConfigureAwait(false);
    }

    private async Task SimulateWorkWithCancellation(int delayMs, CancellationToken cancellationToken)
    {
        await Task.Delay(delayMs, cancellationToken);
    }

    private Task<string> GetDataWithTaskAsync(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return Task.FromResult(value); // Always allocates a Task
        }
        
        return GetDataFromSourceAsync(key);
    }

    private ValueTask<string> GetDataWithValueTaskAsync(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return ValueTask.FromResult(value); // No allocation for synchronous path
        }
        
        return new ValueTask<string>(GetDataFromSourceAsync(key));
    }

    private async Task<string> GetDataFromSourceAsync(string key)
    {
        // Simulate async data retrieval
        await Task.Delay(1);
        var result = $"Fresh data for {key}";
        _cache[key] = result;
        return result;
    }
}

/// <summary>
/// Benchmark adicional para demonstrar impactos específicos de deadlock prevention
/// </summary>
[MemoryDiagnoser]
public class DeadlockPreventionBenchmarks
{
    [Benchmark(Baseline = true)]
    public async Task ChainedAsyncCallsWithoutConfigureAwait()
    {
        await Level1Async();
    }

    [Benchmark]
    public async Task ChainedAsyncCallsWithConfigureAwait()
    {
        await Level1AsyncSafe();
    }

    private async Task Level1Async()
    {
        await Level2Async();
    }

    private async Task Level2Async()
    {
        await Level3Async();
    }

    private async Task Level3Async()
    {
        await Task.Delay(10);
    }

    private async Task Level1AsyncSafe()
    {
        await Level2AsyncSafe().ConfigureAwait(false);
    }

    private async Task Level2AsyncSafe()
    {
        await Level3AsyncSafe().ConfigureAwait(false);
    }

    private async Task Level3AsyncSafe()
    {
        await Task.Delay(10).ConfigureAwait(false);
    }
}

/// <summary>
/// Demonstra custos de diferentes padrões de tratamento de exceção
/// </summary>
[MemoryDiagnoser]
public class ExceptionHandlingBenchmarks
{
    private readonly Random _random = new(42);

    [Benchmark(Baseline = true)]
    public async Task NoExceptionHandling()
    {
        for (int i = 0; i < 100; i++)
        {
            await SafeOperationAsync();
        }
    }

    [Benchmark]
    public async Task WithTryCatchPerCall()
    {
        for (int i = 0; i < 100; i++)
        {
            try
            {
                await SafeOperationAsync();
            }
            catch (Exception)
            {
                // Handle exception
            }
        }
    }

    [Benchmark]
    public async Task WithTryCatchAroundLoop()
    {
        try
        {
            for (int i = 0; i < 100; i++)
            {
                await SafeOperationAsync();
            }
        }
        catch (Exception)
        {
            // Handle exception
        }
    }

    [Benchmark]
    public async Task WithTaskWhenAllAndExceptionHandling()
    {
        var tasks = new List<Task>();
        
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(SafeOperationWithExceptionHandlingAsync());
        }

        await Task.WhenAll(tasks);
    }

    private async Task SafeOperationAsync()
    {
        await Task.Delay(1);
    }

    private async Task SafeOperationWithExceptionHandlingAsync()
    {
        try
        {
            await Task.Delay(1);
        }
        catch (Exception)
        {
            // Handle any exception
        }
    }
}
