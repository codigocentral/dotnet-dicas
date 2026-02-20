using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Runtime.InteropServices;

BenchmarkRunner.Run<SpanAccessBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class SpanAccessBenchmark
{
    private List<int> _data = null!;
    private const int Size = 10000;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(1, Size).ToList();
    }

    [Benchmark(Baseline = true)]
    public long SumWithIndexer()
    {
        long sum = 0;
        for (int i = 0; i < _data.Count; i++)
        {
            sum += _data[i];
        }
        return sum;
    }

    [Benchmark]
    public long SumWithEnumerator()
    {
        long sum = 0;
        foreach (var item in _data)
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark]
    public long SumWithForEach()
    {
        long sum = 0;
        _data.ForEach(x => sum += x);
        return sum;
    }

    [Benchmark]
    public long SumWithSpan()
    {
        var span = CollectionsMarshal.AsSpan(_data);
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i];
        }
        return sum;
    }

    [Benchmark]
    public long SumWithSpanForeach()
    {
        var span = CollectionsMarshal.AsSpan(_data);
        long sum = 0;
        foreach (var item in span)
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark]
    public void ProcessWithIndexer()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data[i] % 2 == 0)
                _data[i] *= 2;
        }
    }

    [Benchmark]
    public void ProcessWithSpan()
    {
        var span = CollectionsMarshal.AsSpan(_data);
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] % 2 == 0)
                span[i] *= 2;
        }
    }

    [Benchmark]
    public int FindFirstEvenWithIndexer()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data[i] % 2 == 0)
                return _data[i];
        }
        return -1;
    }

    [Benchmark]
    public int FindFirstEvenWithSpan()
    {
        var span = CollectionsMarshal.AsSpan(_data);
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] % 2 == 0)
                return span[i];
        }
        return -1;
    }

    [Benchmark]
    public int[] CopyToArrayWithToArray()
    {
        return _data.ToArray();
    }

    [Benchmark]
    public int[] CopyToArrayWithSpan()
    {
        var span = CollectionsMarshal.AsSpan(_data);
        return span.ToArray();
    }
}
