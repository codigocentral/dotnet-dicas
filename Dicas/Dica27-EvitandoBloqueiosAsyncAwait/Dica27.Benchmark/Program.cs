using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<AsyncPatternsBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class AsyncPatternsBenchmark
{
    private const int IterationCount = 100;

    [Benchmark(Baseline = true)]
    public async Task SequentialProcessing()
    {
        for (int i = 0; i < IterationCount; i++)
        {
            await DoWorkAsync(10);
        }
    }

    [Benchmark]
    public async Task ParallelProcessingWithWhenAll()
    {
        var tasks = new Task[IterationCount];
        for (int i = 0; i < IterationCount; i++)
        {
            tasks[i] = DoWorkAsync(10);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task BatchProcessing()
    {
        const int batchSize = 10;
        for (int i = 0; i < IterationCount; i += batchSize)
        {
            var batchTasks = new Task[Math.Min(batchSize, IterationCount - i)];
            for (int j = 0; j < batchTasks.Length; j++)
            {
                batchTasks[j] = DoWorkAsync(10);
            }
            await Task.WhenAll(batchTasks);
        }
    }

    [Benchmark]
    public async Task ConfigureAwaitFalse()
    {
        for (int i = 0; i < IterationCount; i++)
        {
            await DoWorkWithConfigureAwaitAsync(10);
        }
    }

    [Benchmark]
    public async Task TaskRunForCpuBound()
    {
        var tasks = new Task<int>[IterationCount];
        for (int i = 0; i < IterationCount; i++)
        {
            tasks[i] = Task.Run(() => CpuBoundWork(1000));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task DirectCpuBoundInAsync()
    {
        var tasks = new Task<int>[IterationCount];
        for (int i = 0; i < IterationCount; i++)
        {
            tasks[i] = Task.FromResult(CpuBoundWork(1000));
        }
        await Task.WhenAll(tasks);
    }

    private async Task DoWorkAsync(int delayMs)
    {
        await Task.Delay(delayMs);
    }

    private async Task DoWorkWithConfigureAwaitAsync(int delayMs)
    {
        await Task.Delay(delayMs).ConfigureAwait(false);
    }

    private int CpuBoundWork(int iterations)
    {
        int result = 0;
        for (int i = 0; i < iterations; i++)
        {
            result += i * i % 1000;
        }
        return result;
    }
}
