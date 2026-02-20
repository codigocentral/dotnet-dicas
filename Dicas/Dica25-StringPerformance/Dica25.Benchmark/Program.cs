using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Text;

namespace Dica25.Benchmark;

/// <summary>
/// Benchmarks para comparar performance de diferentes técnicas de concatenação de strings
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .AddJob(Job.Default.WithId("Default"));

        BenchmarkRunner.Run<StringPerformanceBenchmarks>(config);
    }
}

[MemoryDiagnoser]
[SimpleJob]
public class StringPerformanceBenchmarks
{
    [Params(5, 50, 500)]
    public int ItemCount { get; set; }

    private string[] _items = Array.Empty<string>();
    private string _name = string.Empty;
    private int _age;
    private string _city = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _items = Enumerable.Range(1, ItemCount)
            .Select(i => $"Item {i}")
            .ToArray();
        
        _name = "João Silva";
        _age = 30;
        _city = "São Paulo";
    }

    /// <summary>
    /// Benchmarks para concatenação simples (poucos elementos)
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Simple")]
    public string StringInterpolation_Simple()
    {
        return $"Olá, {_name}! Você tem {_age} anos e mora em {_city}.";
    }

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public string StringConcat_Simple()
    {
        return string.Concat("Olá, ", _name, "! Você tem ", _age.ToString(), " anos e mora em ", _city, ".");
    }

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public string StringConcatenation_Simple()
    {
        return "Olá, " + _name + "! Você tem " + _age + " anos e mora em " + _city + ".";
    }

    [Benchmark]
    [BenchmarkCategory("Simple")]
    public string StringBuilder_Simple()
    {
        var sb = new StringBuilder();
        sb.Append("Olá, ");
        sb.Append(_name);
        sb.Append("! Você tem ");
        sb.Append(_age);
        sb.Append(" anos e mora em ");
        sb.Append(_city);
        sb.Append(".");
        return sb.ToString();
    }

    /// <summary>
    /// Benchmarks para concatenação em loop
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Loop")]
    public string StringBuilder_Loop()
    {
        var sb = new StringBuilder(_items.Length * 20); // Pre-allocate
        sb.AppendLine("Lista de items:");
        
        foreach (var item in _items)
        {
            sb.Append("- ");
            sb.AppendLine(item);
        }
        
        return sb.ToString();
    }

    [Benchmark]
    [BenchmarkCategory("Loop")]
    public string StringJoin_Loop()
    {
        return "Lista de items:\n" + string.Join("\n", _items.Select(item => $"- {item}"));
    }

    [Benchmark]
    [BenchmarkCategory("Loop")]
    public string StringConcatenation_Loop()
    {
        var result = "Lista de items:\n";
        
        foreach (var item in _items)
        {
            result += $"- {item}\n";
        }
        
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("Loop")]
    public string StringBuilder_NoCapacity_Loop()
    {
        var sb = new StringBuilder(); // No pre-allocation
        sb.AppendLine("Lista de items:");
        
        foreach (var item in _items)
        {
            sb.Append("- ");
            sb.AppendLine(item);
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Benchmarks para formatação com números
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Formatting")]
    public string StringInterpolation_Formatting()
    {
        var value = 12345.67m;
        var date = DateTime.Now;
        
        return $"Valor: {value:C} em {date:dd/MM/yyyy HH:mm}";
    }

    [Benchmark]
    [BenchmarkCategory("Formatting")]
    public string StringFormat_Formatting()
    {
        var value = 12345.67m;
        var date = DateTime.Now;
        
        return string.Format("Valor: {0:C} em {1:dd/MM/yyyy HH:mm}", value, date);
    }

    [Benchmark]
    [BenchmarkCategory("Formatting")]
    public string StringBuilder_Formatting()
    {
        var value = 12345.67m;
        var date = DateTime.Now;
        
        var sb = new StringBuilder();
        sb.Append("Valor: ");
        sb.Append(value.ToString("C"));
        sb.Append(" em ");
        sb.Append(date.ToString("dd/MM/yyyy HH:mm"));
        
        return sb.ToString();
    }

    /// <summary>
    /// Benchmarks para diferentes tipos de join
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Join")]
    public string StringJoin_Comma()
    {
        return string.Join(", ", _items);
    }

    [Benchmark]
    [BenchmarkCategory("Join")]
    public string StringBuilder_ManualJoin()
    {
        var sb = new StringBuilder(_items.Length * 15);
        
        for (int i = 0; i < _items.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(_items[i]);
        }
        
        return sb.ToString();
    }

    [Benchmark]
    [BenchmarkCategory("Join")]
    public string LINQ_Aggregate()
    {
        return _items.Aggregate((a, b) => a + ", " + b);
    }

    /// <summary>
    /// Benchmarks para cenários de construção de JSON simples
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("JSON")]
    public string StringInterpolation_JSON()
    {
        return $"{{\"name\":\"{_name}\",\"age\":{_age},\"city\":\"{_city}\"}}";
    }

    [Benchmark]
    [BenchmarkCategory("JSON")]
    public string StringBuilder_JSON()
    {
        var sb = new StringBuilder();
        sb.Append("{\"name\":\"");
        sb.Append(_name);
        sb.Append("\",\"age\":");
        sb.Append(_age);
        sb.Append(",\"city\":\"");
        sb.Append(_city);
        sb.Append("\"}");
        return sb.ToString();
    }

    [Benchmark]
    [BenchmarkCategory("JSON")]
    public string StringConcat_JSON()
    {
        return string.Concat(
            "{\"name\":\"", _name,
            "\",\"age\":", _age.ToString(),
            ",\"city\":\"", _city, "\"}"
        );
    }
}
