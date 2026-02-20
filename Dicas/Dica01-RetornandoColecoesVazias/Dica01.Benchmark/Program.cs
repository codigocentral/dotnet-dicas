using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Linq;

/*
 * Benchmark: Dica 1 - Retornando Coleções Vazias
 * 
 * Este benchmark demonstra a diferença de performance entre usar Array.Empty<T>() / Enumerable.Empty<T>()
 * versus criar novas instâncias de coleções vazias a cada chamada.
 */

BenchmarkRunner.Run<EmptyCollectionsBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class EmptyCollectionsBenchmark
{
    private const int Iterations = 1000;

    [Benchmark(Baseline = true)]
    public void ArrayEmpty_Recommended()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var empty = GetEmptyArrayRecommended();
            _ = empty.Length; // Simula uso
        }
    }

    [Benchmark]
    public void ArrayNew_Problematic()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var empty = GetEmptyArrayProblematic();
            _ = empty.Length; // Simula uso
        }
    }

    [Benchmark]
    public void EnumerableEmpty_Recommended()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var empty = GetEmptyEnumerableRecommended();
            _ = empty.Count(); // Simula uso
        }
    }

    [Benchmark]
    public void ListNew_Problematic()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var empty = GetEmptyListProblematic();
            _ = empty.Count; // Simula uso
        }
    }

    // ✅ Métodos recomendados - reutilizam instâncias
    private int[] GetEmptyArrayRecommended() => Array.Empty<int>();
    private IEnumerable<string> GetEmptyEnumerableRecommended() => Enumerable.Empty<string>();

    // ❌ Métodos problemáticos - criam novas instâncias
    private int[] GetEmptyArrayProblematic() => new int[0];
    private List<string> GetEmptyListProblematic() => new List<string>();
}
