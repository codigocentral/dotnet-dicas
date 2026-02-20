using System.Collections.Immutable;
using System.Text.Json;

namespace Dica59.TargetTypedNew;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("==== Dica 59: Target-Typed New Expressions - C# 9+ ====\n");

        Console.WriteLine("🎯 Target-Typed New simplifica a criação de objetos!");
        Console.WriteLine("Menos código repetitivo, mais legibilidade...\n");

        // ===== DEMONSTRAÇÃO 1: BÁSICOS DE TARGET-TYPED NEW =====
        Console.WriteLine("1. Básicos do Target-Typed New");
        Console.WriteLine("------------------------------");

        // Forma tradicional vs Target-Typed
        List<string> traditionalList = new List<string> { "apple", "banana", "cherry" };
        List<string> targetTypedList = new() { "apple", "banana", "cherry" };

        Dictionary<string, int> traditionalDict = new Dictionary<string, int>
        {
            ["apple"] = 1,
            ["banana"] = 2
        };
        Dictionary<string, int> targetTypedDict = new()
        {
            ["apple"] = 1,
            ["banana"] = 2
        };

        Console.WriteLine($"✅ Lista tradicional: {string.Join(", ", traditionalList)}");
        Console.WriteLine($"✅ Lista target-typed: {string.Join(", ", targetTypedList)}");
        Console.WriteLine($"✅ Dicionário target-typed: {string.Join(", ", targetTypedDict.Select(kv => $"{kv.Key}={kv.Value}"))}\n");

        // ===== DEMONSTRAÇÃO 2: COM CLASSES E RECORDS =====
        Console.WriteLine("2. Target-Typed New com Classes e Records");
        Console.WriteLine("------------------------------------------");

        // Com classes customizadas
        Person traditionalPerson = new Person("João", 30, "Desenvolvedor");
        Person targetTypedPerson = new("Maria", 25, "Designer");

        // Com records
        Product traditionalProduct = new Product { Id = 1, Name = "Laptop", Price = 2500.00m };
        Product targetTypedProduct = new() { Id = 2, Name = "Mouse", Price = 50.00m };

        // Com init-only properties
        ConfigurationOptions traditionalConfig = new ConfigurationOptions
        {
            DatabaseConnection = "server=localhost;database=test",
            ApiKey = "abc123",
            TimeoutSeconds = 30
        };
        ConfigurationOptions targetTypedConfig = new()
        {
            DatabaseConnection = "server=prod;database=main",
            ApiKey = "xyz789",
            TimeoutSeconds = 60
        };

        Console.WriteLine($"✅ Person target-typed: {targetTypedPerson}");
        Console.WriteLine($"✅ Product target-typed: {targetTypedProduct}");
        Console.WriteLine($"✅ Config target-typed: {targetTypedConfig}\n");

        // ===== DEMONSTRAÇÃO 3: COM ARRAYS E SPANS =====
        Console.WriteLine("3. Target-Typed New com Arrays e Spans");
        Console.WriteLine("---------------------------------------");

        // Arrays
        int[] traditionalArray = new int[] { 1, 2, 3, 4, 5 };
        int[] targetTypedArray = new[] { 1, 2, 3, 4, 5 };  // Já existia
        // Note: arrays não suportam target-typed new diretamente, mas lists sim
        List<int> fullyTargetTyped = new() { 1, 2, 3, 4, 5 };  // C# 9+

        // Arrays multidimensionais
        int[,] traditionalMatrix = new int[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
        int[,] targetTypedMatrix = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        // Com tipos complexos
        Person[] peopleArray = new Person[]
        {
            new("Alice", 28, "Manager"),
            new("Bob", 32, "Developer"),
            new("Carol", 29, "Designer")
        };

        Console.WriteLine($"✅ Array target-typed: [{string.Join(", ", fullyTargetTyped)}]");
        Console.WriteLine($"✅ Pessoas: {peopleArray.Length} pessoas criadas\n");

        // ===== DEMONSTRAÇÃO 4: EM EXPRESSÕES E CONDICIONAIS =====
        Console.WriteLine("4. Target-Typed New em Expressões");
        Console.WriteLine("----------------------------------");

        // Em ternários
        string input = "valid";
        ResponseData result = input == "valid" 
            ? new() { Status = "Success", Message = "Valid input" }
            : new() { Status = "Error", Message = "Invalid input" };

        // Em switch expressions
        var responseType = "json";
        HttpResponse response = responseType switch
        {
            "json" => new() { ContentType = "application/json", Body = "{\"status\":\"ok\"}" },
            "xml" => new() { ContentType = "application/xml", Body = "<status>ok</status>" },
            _ => new() { ContentType = "text/plain", Body = "status=ok" }
        };

        // Em métodos que retornam collections concretas
        List<string> GetDefaultList() => new() { "default", "items" };
        Dictionary<string, object> GetDefaultDict() => new() { ["key"] = "value" };

        Console.WriteLine($"✅ Resultado condicional: {result}");
        Console.WriteLine($"✅ Response por tipo: {response}");
        Console.WriteLine($"✅ Lista default: {string.Join(", ", GetDefaultList())}");
        Console.WriteLine($"✅ Dict default: {string.Join(", ", GetDefaultDict().Select(kv => $"{kv.Key}={kv.Value}"))}\n");

        // ===== DEMONSTRAÇÃO 5: COM COLLECTIONS IMUTÁVEIS =====
        Console.WriteLine("5. Target-Typed New com Collections Imutáveis");
        Console.WriteLine("----------------------------------------------");

        // ImmutableList - usando factory methods
        ImmutableList<string> traditionalImmutableList = ImmutableList.Create("a", "b", "c");
        ImmutableList<string> targetTypedImmutableList = ImmutableList.CreateRange(new[] { "x", "y", "z" });

        // ImmutableDictionary - usando builder pattern
        var traditionalBuilder = ImmutableDictionary.CreateBuilder<string, int>();
        traditionalBuilder.Add("one", 1);
        traditionalBuilder.Add("two", 2);
        ImmutableDictionary<string, int> traditionalImmutableDict = traditionalBuilder.ToImmutable();

        var builder = ImmutableDictionary.CreateBuilder<string, int>();
        builder.Add("three", 3);
        builder.Add("four", 4);
        ImmutableDictionary<string, int> targetTypedImmutableDict = builder.ToImmutable();

        // ImmutableHashSet - usando CreateRange
        ImmutableHashSet<int> numbers = ImmutableHashSet.CreateRange(new[] { 1, 2, 3, 2, 1 }); // Remove duplicatas

        Console.WriteLine($"✅ ImmutableList: [{string.Join(", ", targetTypedImmutableList)}]");
        Console.WriteLine($"✅ ImmutableHashSet: [{string.Join(", ", numbers)}]\n");

        // ===== DEMONSTRAÇÃO 6: EM PADRÕES COMPLEXOS =====
        Console.WriteLine("6. Padrões Complexos com Target-Typed New");
        Console.WriteLine("-----------------------------------------");

        // Factory methods
        var apiClient = CreateApiClient("https://api.example.com", new()
        {
            Timeout = TimeSpan.FromSeconds(30),
            RetryCount = 3,
            ApiKey = "secret-key"
        });

        // Builder pattern
        var queryBuilder = new SqlQueryBuilder()
            .Select("Name", "Age")
            .From("Users")
            .Where(new() { Field = "Age", Operator = ">", Value = 18 })
            .OrderBy("Name");

        // Fluent APIs
        var pipeline = new DataPipeline()
            .AddProcessor(new() { Type = "Filter", Config = new() { ["field"] = "status", ["value"] = "active" } })
            .AddProcessor(new() { Type = "Transform", Config = new() { ["format"] = "json" } })
            .Build();

        Console.WriteLine($"✅ API Client criado: {apiClient.BaseUrl}");
        Console.WriteLine($"✅ Query: {queryBuilder.Build()}");
        Console.WriteLine($"✅ Pipeline: {pipeline.ProcessorCount} processadores\n");

        // ===== DEMONSTRAÇÃO 7: PERFORMANCE E READABILITY =====
        Console.WriteLine("7. Comparação de Performance e Legibilidade");
        Console.WriteLine("--------------------------------------------");

        // Medindo tempo de criação (diferença mínima)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        // Tradicional
        for (int i = 0; i < 100_000; i++)
        {
            var traditional = new List<int> { i, i + 1, i + 2 };
        }
        sw.Stop();
        var traditionalTime = sw.ElapsedMilliseconds;

        sw.Restart();
        // Target-typed
        for (int i = 0; i < 100_000; i++)
        {
            List<int> targetTyped = new() { i, i + 1, i + 2 };
        }
        sw.Stop();
        var targetTypedTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"✅ Tempo tradicional: {traditionalTime} ms");
        Console.WriteLine($"✅ Tempo target-typed: {targetTypedTime} ms");
        Console.WriteLine($"✅ Diferença: Praticamente idêntica (mesmo IL gerado)\n");

        // ===== DEMONSTRAÇÃO 8: CASOS ESPECIAIS E LIMITAÇÕES =====
        Console.WriteLine("8. Casos Especiais e Limitações");
        Console.WriteLine("--------------------------------");

        // Funciona com var (C# 10+)
        var autoVar = new[] { 1, 2, 3 };  // int[]
        // var targetVar = new() { 1, 2, 3 }; // Erro: needs explicit type - comentado para demonstração
        List<int> explicitTargetVar = new() { 1, 2, 3 }; // Funciona com tipo explícito

        // Funciona em expressões mais complexas
        ProcessData(new() { Id = 1, Data = "test" });

        // Funciona com nullable reference types
        Person? nullablePerson = condition ? new("Test", 0, "Job") : null;

        // Anonymous types não funcionam (ainda precisam de new)
        var anonymous = new { Name = "Test", Value = 42 };

        Console.WriteLine("✅ Casos especiais demonstrados");
        Console.WriteLine("⚠️  Limitação: não funciona com tipos anônimos");
        Console.WriteLine("⚠️  Limitação: precisa de tipo explícito (não apenas var)\n");

        Console.WriteLine("=== RESUMO: Target-Typed New ===");
        Console.WriteLine("✅ VANTAGENS:");
        Console.WriteLine("   • Menos código repetitivo (DRY)");
        Console.WriteLine("   • Melhor legibilidade em tipos longos");
        Console.WriteLine("   • Funciona com generics complexos");
        Console.WriteLine("   • Zero overhead de performance");
        Console.WriteLine("   • Suporte a collections imutáveis");
        Console.WriteLine();
        Console.WriteLine("🎯 CASOS DE USO:");
        Console.WriteLine("   • Collections com tipos longos");
        Console.WriteLine("   • Factory methods e builders");
        Console.WriteLine("   • Expressões condicionais");
        Console.WriteLine("   • APIs fluentes");
        Console.WriteLine("   • Configurações e DTOs");
        Console.WriteLine();
        Console.WriteLine("⚠️  CONSIDERAÇÕES:");
        Console.WriteLine("   • C# 9+ necessário");
        Console.WriteLine("   • Tipo de destino deve ser inferível");
        Console.WriteLine("   • Não funciona com tipos anônimos");
    }

    // Método auxiliar para demonstrar var com target-typed new
    private static bool condition = true;

    // Método auxiliar para demonstração
    private static void ProcessData(DataItem item)
    {
        Console.WriteLine($"   Processando: {item}");
    }

    // Factory method para demonstração
    private static ApiClient CreateApiClient(string baseUrl, ApiClientOptions options)
    {
        return new ApiClient(baseUrl, options);
    }
}

// ===== CLASSES E RECORDS PARA DEMONSTRAÇÃO =====

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

    public override string ToString() => 
        $"Config(DB: {DatabaseConnection[..10]}..., Timeout: {TimeoutSeconds}s)";
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

public record DataItem
{
    public int Id { get; init; }
    public string Data { get; init; } = "";
    
    public override string ToString() => $"DataItem(Id: {Id}, Data: {Data})";
}

public record ProcessorConfig
{
    public string Type { get; init; } = "";
    public Dictionary<string, object> Config { get; init; } = new();
}

public record WhereClause
{
    public string Field { get; init; } = "";
    public string Operator { get; init; } = "";
    public object Value { get; init; } = "";
}

public class ApiClientOptions
{
    public TimeSpan Timeout { get; init; }
    public int RetryCount { get; init; }
    public string ApiKey { get; init; } = "";
}

public class ApiClient
{
    public string BaseUrl { get; }
    public ApiClientOptions Options { get; }

    public ApiClient(string baseUrl, ApiClientOptions options)
    {
        BaseUrl = baseUrl;
        Options = options;
    }
}

public class SqlQueryBuilder
{
    private readonly List<string> _selectFields = new();
    private string _fromTable = "";
    private readonly List<WhereClause> _whereClauses = new();
    private readonly List<string> _orderByFields = new();

    public SqlQueryBuilder Select(params string[] fields)
    {
        _selectFields.AddRange(fields);
        return this;
    }

    public SqlQueryBuilder From(string table)
    {
        _fromTable = table;
        return this;
    }

    public SqlQueryBuilder Where(WhereClause clause)
    {
        _whereClauses.Add(clause);
        return this;
    }

    public SqlQueryBuilder OrderBy(string field)
    {
        _orderByFields.Add(field);
        return this;
    }

    public string Build()
    {
        var query = $"SELECT {string.Join(", ", _selectFields)} FROM {_fromTable}";
        
        if (_whereClauses.Any())
        {
            var whereConditions = _whereClauses.Select(w => $"{w.Field} {w.Operator} {w.Value}");
            query += $" WHERE {string.Join(" AND ", whereConditions)}";
        }
        
        if (_orderByFields.Any())
        {
            query += $" ORDER BY {string.Join(", ", _orderByFields)}";
        }
        
        return query;
    }
}

public class DataPipeline
{
    private readonly List<ProcessorConfig> _processors = new();

    public DataPipeline AddProcessor(ProcessorConfig processor)
    {
        _processors.Add(processor);
        return this;
    }

    public DataPipeline Build()
    {
        return this;
    }

    public int ProcessorCount => _processors.Count;
}
