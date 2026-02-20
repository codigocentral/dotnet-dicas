using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Linq;

/*
 * Benchmark: Dica 9 - ToList() vs ToArray()
 * 
 * Este benchmark mede a diferença de performance entre ToList() e ToArray()
 * em diferentes cenários e tamanhos de dados.
 */

BenchmarkRunner.Run<ToListVsToArrayBenchmark>();

[MemoryDiagnoser]
[SimpleJob]
public class ToListVsToArrayBenchmark
{
    private IEnumerable<int> _smallData;
    private IEnumerable<int> _mediumData;
    private IEnumerable<int> _largeData;

    [GlobalSetup]
    public void Setup()
    {
        _smallData = Enumerable.Range(1, 100);
        _mediumData = Enumerable.Range(1, 10_000);
        _largeData = Enumerable.Range(1, 100_000);
    }

    // === TESTES COM DADOS PEQUENOS (100 itens) ===
    
    [Benchmark]
    [BenchmarkCategory("Small")]
    public List<int> Small_ToList()
    {
        return _smallData.ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Small")]
    public int[] Small_ToArray()
    {
        return _smallData.ToArray();
    }

    // === TESTES COM DADOS MÉDIOS (10.000 itens) ===
    
    [Benchmark]
    [BenchmarkCategory("Medium")]
    public List<int> Medium_ToList()
    {
        return _mediumData.ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Medium")]
    public int[] Medium_ToArray()
    {
        return _mediumData.ToArray();
    }

    // === TESTES COM DADOS GRANDES (100.000 itens) ===
    
    [Benchmark]
    [BenchmarkCategory("Large")]
    public List<int> Large_ToList()
    {
        return _largeData.ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Large")]
    public int[] Large_ToArray()
    {
        return _largeData.ToArray();
    }

    // === TESTES DE ITERAÇÃO (para medir performance de acesso) ===

    [Benchmark]
    [BenchmarkCategory("Iteration")]
    public long IterateList()
    {
        var list = _mediumData.ToList();
        long sum = 0;
        
        for (int i = 0; i < list.Count; i++)
        {
            sum += list[i];
        }
        
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Iteration")]
    public long IterateArray()
    {
        var array = _mediumData.ToArray();
        long sum = 0;
        
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }
        
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Iteration")]
    public long ForeachList()
    {
        var list = _mediumData.ToList();
        long sum = 0;
        
        foreach (var item in list)
        {
            sum += item;
        }
        
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Iteration")]
    public long ForeachArray()
    {
        var array = _mediumData.ToArray();
        long sum = 0;
        
        foreach (var item in array)
        {
            sum += item;
        }
        
        return sum;
    }

    // === TESTES DE MANIPULAÇÃO (onde List tem vantagem) ===

    [Benchmark]
    [BenchmarkCategory("Manipulation")]
    public List<int> ListManipulation()
    {
        var list = _smallData.ToList();
        
        // Operações que só List pode fazer
        list.Add(999);
        list.Insert(0, -1);
        list.Remove(50);
        list.AddRange(new[] { 1001, 1002, 1003 });
        
        return list;
    }

    // Arrays não podem fazer isso - seria um erro de compilação
    /*
    [Benchmark]
    public int[] ArrayManipulation()
    {
        var array = _smallData.ToArray();
        
        // ❌ Isso não compila:
        // array.Add(999);
        // array.Insert(0, -1);
        // array.Remove(50);
        
        return array;
    }
    */

    // === TESTE DE CONVERSÃO DE CONHECIDA COLLECTION ===

    [Benchmark]
    [BenchmarkCategory("Conversion")]
    public List<int> ListFromKnownCount()
    {
        var source = new List<int>(_mediumData); // Count conhecido
        return source.ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Conversion")]
    public int[] ArrayFromKnownCount()
    {
        var source = new List<int>(_mediumData); // Count conhecido
        return source.ToArray();
    }
}
