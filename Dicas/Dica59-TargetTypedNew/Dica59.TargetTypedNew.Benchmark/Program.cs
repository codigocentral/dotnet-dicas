using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Immutable;

namespace Dica59.TargetTypedNew.Benchmark;

[MemoryDiagnoser]
[SimpleJob]
public class TargetTypedNewBenchmarks
{
    private readonly string[] _testData = Enumerable.Range(0, 1000).Select(i => $"Item{i}").ToArray();
    
    [Benchmark(Baseline = true)]
    public List<string> TraditionalListCreation()
    {
        var list = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            list.Add($"Item{i}");
        }
        return list;
    }

    [Benchmark]
    public List<string> TargetTypedListCreation()
    {
        List<string> list = new();
        for (int i = 0; i < 100; i++)
        {
            list.Add($"Item{i}");
        }
        return list;
    }

    [Benchmark]
    public Dictionary<string, int> TraditionalDictionaryCreation()
    {
        var dict = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            dict[$"Key{i}"] = i;
        }
        return dict;
    }

    [Benchmark]
    public Dictionary<string, int> TargetTypedDictionaryCreation()
    {
        Dictionary<string, int> dict = new();
        for (int i = 0; i < 100; i++)
        {
            dict[$"Key{i}"] = i;
        }
        return dict;
    }

    [Benchmark]
    public Person[] TraditionalArrayCreation()
    {
        return new Person[]
        {
            new Person("Alice", 25, "Developer"),
            new Person("Bob", 30, "Manager"),
            new Person("Carol", 28, "Designer")
        };
    }

    [Benchmark]
    public Person[] TargetTypedArrayCreation()
    {
        Person[] people = new Person[]
        {
            new("Alice", 25, "Developer"),
            new("Bob", 30, "Manager"),
            new("Carol", 28, "Designer")
        };
        return people;
    }

    [Benchmark]
    public ImmutableList<string> TraditionalImmutableListCreation()
    {
        return ImmutableList.Create(_testData.Take(50).ToArray());
    }

    [Benchmark]
    public ImmutableList<string> TargetTypedImmutableListCreation()
    {
        ImmutableList<string> list = ImmutableList.CreateRange(_testData.Take(50));
        return list;
    }

    [Benchmark]
    public ConfigurationOptions TraditionalConfigCreation()
    {
        return new ConfigurationOptions
        {
            DatabaseConnection = "server=localhost;database=test",
            ApiKey = "abc123",
            TimeoutSeconds = 30
        };
    }

    [Benchmark]
    public ConfigurationOptions TargetTypedConfigCreation()
    {
        ConfigurationOptions config = new()
        {
            DatabaseConnection = "server=localhost;database=test",
            ApiKey = "abc123",
            TimeoutSeconds = 30
        };
        return config;
    }

    [Benchmark]
    public List<Product> TraditionalComplexObjectCreation()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 2500.00m },
            new Product { Id = 2, Name = "Mouse", Price = 50.00m },
            new Product { Id = 3, Name = "Keyboard", Price = 150.00m }
        };
    }

    [Benchmark]
    public List<Product> TargetTypedComplexObjectCreation()
    {
        List<Product> products = new()
        {
            new() { Id = 1, Name = "Laptop", Price = 2500.00m },
            new() { Id = 2, Name = "Mouse", Price = 50.00m },
            new() { Id = 3, Name = "Keyboard", Price = 150.00m }
        };
        return products;
    }

    [Benchmark]
    public ResponseData ConditionalTraditionalCreation()
    {
        var condition = DateTime.Now.Millisecond % 2 == 0;
        return condition 
            ? new ResponseData { Status = "Success", Message = "All good" }
            : new ResponseData { Status = "Error", Message = "Something failed" };
    }

    [Benchmark]
    public ResponseData ConditionalTargetTypedCreation()
    {
        var condition = DateTime.Now.Millisecond % 2 == 0;
        ResponseData response = condition 
            ? new() { Status = "Success", Message = "All good" }
            : new() { Status = "Error", Message = "Something failed" };
        return response;
    }

    [Benchmark]
    public HttpResponse SwitchExpressionTraditional()
    {
        var responseType = "json";
        return responseType switch
        {
            "json" => new HttpResponse { ContentType = "application/json", Body = "{\"status\":\"ok\"}" },
            "xml" => new HttpResponse { ContentType = "application/xml", Body = "<status>ok</status>" },
            _ => new HttpResponse { ContentType = "text/plain", Body = "status=ok" }
        };
    }

    [Benchmark]
    public HttpResponse SwitchExpressionTargetTyped()
    {
        var responseType = "json";
        HttpResponse response = responseType switch
        {
            "json" => new() { ContentType = "application/json", Body = "{\"status\":\"ok\"}" },
            "xml" => new() { ContentType = "application/xml", Body = "<status>ok</status>" },
            _ => new() { ContentType = "text/plain", Body = "status=ok" }
        };
        return response;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Dica 59: Target-Typed New - Performance Benchmarks ===\n");
        
        var summary = BenchmarkRunner.Run<TargetTypedNewBenchmarks>();
        
        Console.WriteLine("\n=== ANÁLISE DOS RESULTADOS ===");
        Console.WriteLine("✅ Target-Typed New oferece:");
        Console.WriteLine("   • Performance idêntica ao código tradicional");
        Console.WriteLine("   • Mesmo IL gerado pelo compilador");
        Console.WriteLine("   • Zero overhead de runtime");
        Console.WriteLine("   • Melhor legibilidade em tipos complexos");
        Console.WriteLine();
        Console.WriteLine("🎯 BENEFÍCIOS:");
        Console.WriteLine("   • Reduz código repetitivo (DRY)");
        Console.WriteLine("   • Melhor manutenibilidade");
        Console.WriteLine("   • Menos propenso a erros de tipos");
        Console.WriteLine("   • Funciona perfeitamente com generics");
        Console.WriteLine();
        Console.WriteLine("📊 CONCLUSÃO:");
        Console.WriteLine("   • Use Target-Typed New sempre que possível (C# 9+)");
        Console.WriteLine("   • Especialmente útil com tipos longos/complexos");
        Console.WriteLine("   • Sem impacto na performance");
        Console.WriteLine("   • Melhora significativamente a legibilidade");
    }
}

// Types para benchmark
public record Person(string Name, int Age, string Job);

public record Product
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
}

public class ConfigurationOptions
{
    public string DatabaseConnection { get; init; } = "";
    public string ApiKey { get; init; } = "";
    public int TimeoutSeconds { get; init; }
}

public record ResponseData
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

public record HttpResponse
{
    public string ContentType { get; init; } = "";
    public string Body { get; init; } = "";
}
