using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Linq;

/*
 * Benchmark: Dica 4 - Armadilhas de Desempenho do LINQ
 * 
 * Este benchmark demonstra a diferença dramática de performance entre múltipla enumeração
 * versus materialização única de um IEnumerable.
 */

BenchmarkRunner.Run<LinqEnumerationBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class LinqEnumerationBenchmark
{
    private const int DataSize = 100_000;

    [Benchmark(Baseline = true)]
    public (int count, bool hasAny, int max, double avg) MultipleEnumeration_Problematic()
    {
        var data = GetExpensiveData();
        
        // ❌ Cada operação enumera todo o IEnumerable novamente
        var count = data.Count();        // 1ª enumeração completa
        var hasAny = data.Any();         // 2ª enumeração 
        var max = data.Max();            // 3ª enumeração completa
        var avg = data.Average();        // 4ª enumeração completa
        
        return (count, hasAny, max, avg);
    }

    [Benchmark]
    public (int count, bool hasAny, int max, double avg) SingleEnumeration_ToList()
    {
        var data = GetExpensiveData();
        
        // ✅ Materializa uma vez em List
        var materializedData = data.ToList();
        
        // Agora todas as operações usam a lista materializada
        var count = materializedData.Count;      // Propriedade O(1)
        var hasAny = materializedData.Any();     // Opera na lista
        var max = materializedData.Max();       // Opera na lista
        var avg = materializedData.Average();   // Opera na lista
        
        return (count, hasAny, max, avg);
    }

    [Benchmark]
    public (int count, bool hasAny, int max, double avg) SingleEnumeration_ToArray()
    {
        var data = GetExpensiveData();
        
        // ✅ Materializa uma vez em Array
        var materializedData = data.ToArray();
        
        // Agora todas as operações usam o array materializado
        var count = materializedData.Length;    // Propriedade O(1)
        var hasAny = materializedData.Any();    // Opera no array
        var max = materializedData.Max();       // Opera no array
        var avg = materializedData.Average();   // Opera no array
        
        return (count, hasAny, max, avg);
    }

    [Benchmark]
    public (int count, bool hasAny, int max, double avg) OptimizedSinglePass()
    {
        var data = GetExpensiveData();
        
        // ✅ Mais otimizado: calcular tudo em uma única passada
        int count = 0;
        int max = int.MinValue;
        long sum = 0;
        bool hasAny = false;
        
        foreach (var item in data)
        {
            hasAny = true;
            count++;
            if (item > max) max = item;
            sum += item;
        }
        
        double avg = hasAny ? (double)sum / count : 0;
        return (count, hasAny, max, avg);
    }

    // Simula operação cara que retorna IEnumerable
    private IEnumerable<int> GetExpensiveData()
    {
        // Simula processamento caro com yield return
        for (int i = 1; i <= DataSize; i++)
        {
            // Simula operação cara
            var result = i * 2 + (i % 3 == 0 ? 1 : 0);
            yield return result;
        }
    }
}
