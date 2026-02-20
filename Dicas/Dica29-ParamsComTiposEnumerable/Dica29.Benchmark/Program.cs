using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

Console.WriteLine("🚀 Executando benchmarks da Dica 29: Params com Tipos Enumerable");
Console.WriteLine("===================================================================");

BenchmarkRunner.Run<ParamsBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class ParamsBenchmark
{
    private readonly int[] _testData = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Allocation")]
    public int ArrayParams_Traditional()
    {
        return ProcessArray(1, 2, 3, 4, 5);
    }

    [Benchmark]
    [BenchmarkCategory("Allocation")]
    public int SpanParams_Modern()
    {
        return ProcessSpan(1, 2, 3, 4, 5);
    }

    [Benchmark]
    [BenchmarkCategory("Allocation")]
    public int ReadOnlySpanParams_Modern()
    {
        return ProcessReadOnlySpan(1, 2, 3, 4, 5);
    }

    [Benchmark]
    [BenchmarkCategory("Allocation")]
    public int IEnumerableParams_Flexible()
    {
        return ProcessIEnumerable(1, 2, 3, 4, 5);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LargeData")]
    public long ArrayParams_LargeData()
    {
        return ProcessArrayLarge(_testData);
    }

    [Benchmark]
    [BenchmarkCategory("LargeData")]
    public long ReadOnlySpanParams_LargeData()
    {
        return ProcessReadOnlySpanLarge(_testData);
    }

    [Benchmark]
    [BenchmarkCategory("LargeData")]
    public long IEnumerableParams_LargeData()
    {
        return ProcessIEnumerableLarge(_testData);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HotPath")]
    public void ArrayParams_HotPath()
    {
        for (int i = 0; i < 1000; i++)
            ProcessArrayVoid(i, i + 1, i + 2);
    }

    [Benchmark]
    [BenchmarkCategory("HotPath")]
    public void ReadOnlySpanParams_HotPath()
    {
        for (int i = 0; i < 1000; i++)
            ProcessReadOnlySpanVoid(i, i + 1, i + 2);
    }

    // ================================================
    // MÉTODOS DE TESTE - ALLOCATION
    // ================================================

    private static int ProcessArray(params int[] numbers)
    {
        int sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }

    private static int ProcessSpan(params Span<int> numbers)
    {
        int sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }

    private static int ProcessReadOnlySpan(params ReadOnlySpan<int> numbers)
    {
        int sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }

    private static int ProcessIEnumerable(params IEnumerable<int> numbers)
    {
        return numbers.Sum();
    }

    // ================================================
    // MÉTODOS DE TESTE - LARGE DATA
    // ================================================

    private static long ProcessArrayLarge(params int[] numbers)
    {
        long sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }

    private static long ProcessReadOnlySpanLarge(params ReadOnlySpan<int> numbers)
    {
        long sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum;
    }

    private static long ProcessIEnumerableLarge(params IEnumerable<int> numbers)
    {
        return numbers.Sum(x => (long)x);
    }

    // ================================================
    // MÉTODOS DE TESTE - HOT PATH
    // ================================================

    private static void ProcessArrayVoid(params int[] numbers)
    {
        // Simula processamento sem retorno
        var _ = numbers.Length > 0 ? numbers[0] : 0;
    }

    private static void ProcessReadOnlySpanVoid(params ReadOnlySpan<int> numbers)
    {
        // Simula processamento sem retorno
        var _ = numbers.Length > 0 ? numbers[0] : 0;
    }
}

/// <summary>
/// Benchmark específico para string operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class StringParamsBenchmark
{
    [Benchmark(Baseline = true)]
    public string ArrayParams_StringConcat()
    {
        return ConcatArray("Hello", " ", "World", " ", "from", " ", "C#");
    }

    [Benchmark]
    public string ReadOnlySpanParams_StringConcat()
    {
        string[] strings = ["Hello", " ", "World", " ", "from", " ", "C#"];
        return ConcatReadOnlySpan(strings.AsSpan());
    }

    [Benchmark]
    public string IEnumerableParams_StringConcat()
    {
        return ConcatIEnumerable("Hello", " ", "World", " ", "from", " ", "C#");
    }

    private static string ConcatArray(params string[] values)
    {
        return string.Concat(values);
    }

    private static string ConcatReadOnlySpan(ReadOnlySpan<string> values)
    {
        return string.Join("", values.ToArray());
    }

    private static string ConcatIEnumerable(params string[] values)
    {
        return string.Concat(values.AsEnumerable());
    }
}

/// <summary>
/// Benchmark para operações matemáticas de alta performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class MathParamsBenchmark
{
    private readonly double[] _data = Enumerable.Range(1, 100)
        .Select(x => (double)x)
        .ToArray();

    [Benchmark(Baseline = true)]
    public double ArrayParams_Average()
    {
        return CalculateAverageArray(_data);
    }

    [Benchmark]
    public double ReadOnlySpanParams_Average()
    {
        return CalculateAverageReadOnlySpan(_data);
    }

    [Benchmark]
    public double IEnumerableParams_Average()
    {
        return CalculateAverageIEnumerable(_data);
    }

    private static double CalculateAverageArray(params double[] numbers)
    {
        if (numbers.Length == 0) return 0;
        
        double sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum / numbers.Length;
    }

    private static double CalculateAverageReadOnlySpan(params ReadOnlySpan<double> numbers)
    {
        if (numbers.IsEmpty) return 0;
        
        double sum = 0;
        foreach (var num in numbers)
            sum += num;
        return sum / numbers.Length;
    }

    private static double CalculateAverageIEnumerable(params IEnumerable<double> numbers)
    {
        return numbers.Average();
    }
}
