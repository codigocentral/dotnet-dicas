using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ValueTaskBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class ValueTaskBenchmark
{
    private readonly Dictionary<string, string> _cache = new()
    {
        ["key1"] = "cached_value_1",
        ["key2"] = "cached_value_2",
        ["key3"] = "cached_value_3",
        ["key4"] = "cached_value_4",
        ["key5"] = "cached_value_5"
    };

    [Params(0.1, 0.5, 0.9)] // Cache hit ratio
    public double CacheHitRatio { get; set; }

    [Benchmark(Baseline = true)]
    public async Task<int> TaskWithCache()
    {
        int count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await GetValueWithTask($"key{(i % 10) + 1}");
            count += result.Length;
        }
        return count;
    }

    [Benchmark]
    public async Task<int> ValueTaskWithCache()
    {
        int count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await GetValueWithValueTask($"key{(i % 10) + 1}");
            count += result.Length;
        }
        return count;
    }

    [Benchmark]
    public async Task<int> TaskSyncOperations()
    {
        int sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await ValidateWithTask(i);
            sum += result ? 1 : 0;
        }
        return sum;
    }

    [Benchmark]
    public async Task<int> ValueTaskSyncOperations()
    {
        int sum = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await ValidateWithValueTask(i);
            sum += result ? 1 : 0;
        }
        return sum;
    }

    [Benchmark]
    public async Task<int> TaskConditional()
    {
        int count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await ConditionalWithTask(i % 2 == 0);
            count += result;
        }
        return count;
    }

    [Benchmark]
    public async Task<int> ValueTaskConditional()
    {
        int count = 0;
        for (int i = 0; i < 1000; i++)
        {
            var result = await ConditionalWithValueTask(i % 2 == 0);
            count += result;
        }
        return count;
    }

    [Benchmark]
    public async Task<int> TaskFrequentCalls()
    {
        int total = 0;
        var tasks = new Task<int>[100];
        
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = GetNumberWithTask(i);
        }

        var results = await Task.WhenAll(tasks);
        for (int i = 0; i < results.Length; i++)
        {
            total += results[i];
        }
        
        return total;
    }

    [Benchmark]
    public async Task<int> ValueTaskFrequentCalls()
    {
        int total = 0;
        var tasks = new ValueTask<int>[100];
        
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = GetNumberWithValueTask(i);
        }

        for (int i = 0; i < tasks.Length; i++)
        {
            total += await tasks[i];
        }
        
        return total;
    }

    // Task implementations
    private async Task<string> GetValueWithTask(string key)
    {
        if (ShouldHitCache() && _cache.TryGetValue(key, out var cached))
        {
            await Task.Yield(); // Still allocates Task
            return cached;
        }

        await Task.Delay(1); // Simula I/O
        return $"loaded_{key}";
    }

    private async Task<bool> ValidateWithTask(int value)
    {
        if (value % 2 == 0)
        {
            return await Task.FromResult(true); // Always allocates
        }

        await Task.Delay(1);
        return value > 100;
    }

    private async Task<int> ConditionalWithTask(bool condition)
    {
        if (condition)
        {
            return await Task.FromResult(42); // Always allocates
        }

        await Task.Delay(1);
        return 24;
    }

    private async Task<int> GetNumberWithTask(int input)
    {
        if (input % 3 == 0)
        {
            return await Task.FromResult(input * 2); // Always allocates
        }

        await Task.Delay(1);
        return input;
    }

    // ValueTask implementations
    private ValueTask<string> GetValueWithValueTask(string key)
    {
        if (ShouldHitCache() && _cache.TryGetValue(key, out var cached))
        {
            return ValueTask.FromResult(cached); // No allocation!
        }

        return LoadValueAsyncValueTask(key);
    }

    private async ValueTask<string> LoadValueAsyncValueTask(string key)
    {
        await Task.Delay(1);
        return $"loaded_{key}";
    }

    private ValueTask<bool> ValidateWithValueTask(int value)
    {
        if (value % 2 == 0)
        {
            return ValueTask.FromResult(true); // No allocation!
        }

        return ValidateAsyncValueTask(value);
    }

    private async ValueTask<bool> ValidateAsyncValueTask(int value)
    {
        await Task.Delay(1);
        return value > 100;
    }

    private ValueTask<int> ConditionalWithValueTask(bool condition)
    {
        if (condition)
        {
            return ValueTask.FromResult(42); // No allocation!
        }

        return DelayedValueTask();
    }

    private async ValueTask<int> DelayedValueTask()
    {
        await Task.Delay(1);
        return 24;
    }

    private ValueTask<int> GetNumberWithValueTask(int input)
    {
        if (input % 3 == 0)
        {
            return ValueTask.FromResult(input * 2); // No allocation!
        }

        return GetNumberAsyncValueTask(input);
    }

    private async ValueTask<int> GetNumberAsyncValueTask(int input)
    {
        await Task.Delay(1);
        return input;
    }

    private bool ShouldHitCache()
    {
        return Random.Shared.NextDouble() < CacheHitRatio;
    }
}
