using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dica83.NativeAOT;

// JsonSerializerContext para AOT (Source Generator)
[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(UserData))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(object))]
internal partial class JsonContext : JsonSerializerContext
{
}

/// <summary>
/// Dica 83: Native AOT (Ahead-of-Time Compilation)
/// 
/// Demonstra como usar Native AOT para compilar aplicações .NET para binários nativos,
/// eliminando a necessidade da runtime .NET e melhorando drasticamente:
/// - Tempo de inicialização (cold start)
/// - Uso de memória
/// - Tamanho do executável
/// - Segurança e proteção contra engenharia reversa
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Dica 83: Native AOT ===\n");

        // Medir desempenho de inicialização
        var stopwatch = Stopwatch.StartNew();
        
        // Demonstrações básicas
        ShowAOTInfo();
        await DemoStartupPerformance();
        await DemoMemoryUsage();
        await DemoFileOperations();
        await DemoJsonSerialization();
        await DemoReflectionLimitations();
        ShowBestPractices();
        
        stopwatch.Stop();
        Console.WriteLine($"\n⚡ Tempo total de execução: {stopwatch.ElapsedMilliseconds}ms");
        
        return 0;
    }

    /// <summary>
    /// Mostra informações sobre Native AOT
    /// </summary>
    static void ShowAOTInfo()
    {
        Console.WriteLine("🚀 Native AOT - Informações");
        Console.WriteLine("===========================");
        
        Console.WriteLine("📋 Características:");
        Console.WriteLine("• ✅ Compilação para código nativo específico da plataforma");
        Console.WriteLine("• ✅ Não requer runtime .NET instalada");
        Console.WriteLine("• ✅ Inicialização instantânea (sem JIT)");
        Console.WriteLine("• ✅ Menor uso de memória");
        Console.WriteLine("• ✅ Melhor para containers e serverless");
        Console.WriteLine("• ✅ Proteção contra engenharia reversa");
        Console.WriteLine();
        
        Console.WriteLine("⚠️ Limitações:");
        Console.WriteLine("• ❌ Reflection limitada");
        Console.WriteLine("• ❌ Assembly loading dinâmico limitado");
        Console.WriteLine("• ❌ Alguns recursos de runtime não disponíveis");
        Console.WriteLine("• ❌ Tamanho do executável pode ser maior");
        Console.WriteLine();
        
        // Verificar se está rodando como AOT
        var isAOT = !System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported;
        Console.WriteLine($"🔍 Executando como Native AOT: {(isAOT ? "✅ SIM" : "❌ NÃO")}");
        Console.WriteLine($"🔍 Suporte a código dinâmico: {(isAOT ? "❌ NÃO" : "✅ SIM")}");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra a performance de inicialização
    /// </summary>
    static async Task DemoStartupPerformance()
    {
        Console.WriteLine("⚡ Performance de Inicialização");
        Console.WriteLine("==============================");
        
        var processStart = Process.GetCurrentProcess().StartTime;
        var now = DateTime.Now;
        var startupTime = now - processStart;
        
        Console.WriteLine($"Tempo de inicialização do processo: {startupTime.TotalMilliseconds:F2}ms");
        
        // Medir tempo para operações iniciais
        var sw = Stopwatch.StartNew();
        
        // Operações típicas de inicialização
        var list = new List<int>(1000);
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i * i);
        }
        
        var dict = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            dict[$"key_{i}"] = i;
        }
        
        sw.Stop();
        Console.WriteLine($"Tempo para inicializar estruturas de dados: {sw.Elapsed.TotalMicroseconds:F0}μs");
        
        // Simular cold start scenario
        await SimulateColdStart();
        
        Console.WriteLine();
    }

    static async Task SimulateColdStart()
    {
        Console.WriteLine("\n🧊 Simulação de Cold Start:");
        
        var operations = new (string Name, Action Operation)[]
        {
            ("Parsing JSON", () => ParseSampleJson()),
            ("File I/O", () => WriteAndReadFile()),
            ("Math operations", () => PerformMathOperations()),
            ("String processing", () => ProcessStrings())
        };
        
        foreach (var (name, operation) in operations)
        {
            var sw = Stopwatch.StartNew();
            operation();
            sw.Stop();
            Console.WriteLine($"  {name}: {sw.Elapsed.TotalMicroseconds:F0}μs");
        }
        
        await Task.CompletedTask;
    }

    static void ParseSampleJson()
    {
        var json = """{"name": "AOT Test", "value": 42, "items": [1, 2, 3]}""";
        var doc = JsonDocument.Parse(json);
        var name = doc.RootElement.GetProperty("name").GetString();
    }

    static void WriteAndReadFile()
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "AOT Test Content");
        var content = File.ReadAllText(tempFile);
        File.Delete(tempFile);
    }

    static void PerformMathOperations()
    {
        double result = 0;
        for (int i = 0; i < 1000; i++)
        {
            result += Math.Sin(i) * Math.Cos(i);
        }
    }

    static void ProcessStrings()
    {
        var text = "Native AOT is amazing for performance!";
        for (int i = 0; i < 100; i++)
        {
            var processed = text.ToUpper().Replace("AOT", "AHEAD-OF-TIME");
        }
    }

    /// <summary>
    /// Demonstra uso de memória otimizado
    /// </summary>
    static async Task DemoMemoryUsage()
    {
        Console.WriteLine("💾 Uso de Memória Otimizado");
        Console.WriteLine("============================");
        
        var initialMemory = GC.GetTotalMemory(false);
        Console.WriteLine($"Memória inicial: {initialMemory / 1024:N0} KB");
        
        // Alocar e trabalhar com dados
        var data = GenerateLargeDataSet(10000);
        var processedData = ProcessDataSet(data);
        
        var afterProcessing = GC.GetTotalMemory(false);
        Console.WriteLine($"Memória após processamento: {afterProcessing / 1024:N0} KB");
        Console.WriteLine($"Incremento: {(afterProcessing - initialMemory) / 1024:N0} KB");
        
        // Forçar coleta de lixo
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var afterGC = GC.GetTotalMemory(false);
        Console.WriteLine($"Memória após GC: {afterGC / 1024:N0} KB");
        Console.WriteLine($"Memória liberada: {(afterProcessing - afterGC) / 1024:N0} KB");
        
        // Mostrar informações de GC
        ShowGCInfo();
        
        Console.WriteLine();
        await Task.CompletedTask;
    }

    static List<DataPoint> GenerateLargeDataSet(int size)
    {
        var random = new Random(42);
        var data = new List<DataPoint>(size);
        
        for (int i = 0; i < size; i++)
        {
            data.Add(new DataPoint
            {
                Id = i,
                Value = random.NextDouble() * 1000,
                Timestamp = DateTime.Now.AddSeconds(-random.Next(86400)),
                Category = $"Category_{i % 10}"
            });
        }
        
        return data;
    }

    static List<ProcessedData> ProcessDataSet(List<DataPoint> data)
    {
        return data
            .Where(d => d.Value > 500)
            .GroupBy(d => d.Category)
            .Select(g => new ProcessedData
            {
                Category = g.Key,
                Count = g.Count(),
                Average = g.Average(x => x.Value),
                Max = g.Max(x => x.Value)
            })
            .ToList();
    }

    static void ShowGCInfo()
    {
        Console.WriteLine("\n🗑️ Informações do Garbage Collector:");
        Console.WriteLine($"  Gen 0 collections: {GC.CollectionCount(0)}");
        Console.WriteLine($"  Gen 1 collections: {GC.CollectionCount(1)}");
        Console.WriteLine($"  Gen 2 collections: {GC.CollectionCount(2)}");
        
        if (GC.MaxGeneration >= 3)
        {
            Console.WriteLine($"  LOH collections: {GC.CollectionCount(3)}");
        }
    }

    /// <summary>
    /// Demonstra operações de arquivo otimizadas
    /// </summary>
    static async Task DemoFileOperations()
    {
        Console.WriteLine("📁 Operações de Arquivo Otimizadas");
        Console.WriteLine("===================================");
        
        var tempDir = Path.Combine(Path.GetTempPath(), "aot-demo");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Teste de escrita de arquivo
            var sw = Stopwatch.StartNew();
            var testFile = Path.Combine(tempDir, "aot-test.txt");
            
            var content = string.Join('\n', Enumerable.Range(1, 1000).Select(i => $"Line {i}: Native AOT is fast!"));
            await File.WriteAllTextAsync(testFile, content);
            
            sw.Stop();
            Console.WriteLine($"Escrita de arquivo (1000 linhas): {sw.Elapsed.TotalMicroseconds:F0}μs");
            
            // Teste de leitura de arquivo
            sw.Restart();
            var readContent = await File.ReadAllTextAsync(testFile);
            var lines = readContent.Split('\n');
            sw.Stop();
            Console.WriteLine($"Leitura de arquivo ({lines.Length} linhas): {sw.Elapsed.TotalMicroseconds:F0}μs");
            
            // Teste de operações de diretório
            sw.Restart();
            var files = Directory.GetFiles(tempDir);
            var dirs = Directory.GetDirectories(tempDir);
            sw.Stop();
            Console.WriteLine($"Listagem de diretório: {sw.Elapsed.TotalMicroseconds:F0}μs");
            
            // Informações do arquivo
            var fileInfo = new FileInfo(testFile);
            Console.WriteLine($"Tamanho do arquivo: {fileInfo.Length} bytes");
            Console.WriteLine($"Última modificação: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        }
        finally
        {
            // Limpeza
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstra serialização JSON com Source Generators (AOT-friendly)
    /// </summary>
    static async Task DemoJsonSerialization()
    {
        Console.WriteLine("🔄 Serialização JSON (AOT-friendly)");
        Console.WriteLine("====================================");
        
        // Criar dados de teste
        var testData = new ApiResponse
        {
            Success = true,
            Message = "Native AOT serialization test",
            Data = new UserData
            {
                Id = 12345,
                Name = "João Silva",
                Email = "joao@exemplo.com",
                CreatedAt = DateTime.Now,
                Roles = ["Admin", "User"]
            },
            Metadata = new Dictionary<string, object>
            {
                ["version"] = "1.0",
                ["timestamp"] = DateTime.Now.ToString("O"),
                ["server"] = Environment.MachineName
            }
        };
        
        // Serialização com Source Generator (AOT-friendly)
        var sw = Stopwatch.StartNew();
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = JsonContext.Default
        };
        
        var json = JsonSerializer.Serialize(testData, JsonContext.Default.ApiResponse);
        sw.Stop();
        Console.WriteLine($"Serialização: {sw.Elapsed.TotalMicroseconds:F0}μs");
        
        // Deserialização
        sw.Restart();
        var deserializedData = JsonSerializer.Deserialize(json, JsonContext.Default.ApiResponse);
        sw.Stop();
        Console.WriteLine($"Deserialização: {sw.Elapsed.TotalMicroseconds:F0}μs");
        
        Console.WriteLine($"JSON gerado: {json.Length} caracteres");
        Console.WriteLine($"Dados deserializados: {deserializedData?.Data?.Name}");
        
        // Demonstrar performance em lote
        await DemoBatchJsonProcessing(testData);
        
        Console.WriteLine();
    }

    static async Task DemoBatchJsonProcessing(ApiResponse template)
    {
        Console.WriteLine("\n📊 Processamento em lote:");
        
        const int batchSize = 1000;
        var items = new List<ApiResponse>(batchSize);
        
        // Gerar lote de dados
        for (int i = 0; i < batchSize; i++)
        {
            items.Add(new ApiResponse
            {
                Success = true,
                Message = $"Item {i}",
                Data = new UserData
                {
                    Id = i,
                    Name = $"User {i}",
                    Email = $"user{i}@exemplo.com",
                    CreatedAt = DateTime.Now.AddDays(-i),
                    Roles = ["User"]
                }
            });
        }
        
        // Serializar em lote
        var sw = Stopwatch.StartNew();
        var jsonResults = new List<string>(batchSize);
        
        foreach (var item in items)
        {
            jsonResults.Add(JsonSerializer.Serialize(item, JsonContext.Default.ApiResponse));
        }
        
        sw.Stop();
        Console.WriteLine($"  Serialização de {batchSize} itens: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Média por item: {sw.Elapsed.TotalMicroseconds / (double)batchSize:F2}μs");
        
        var totalSize = jsonResults.Sum(j => j.Length);
        Console.WriteLine($"  Tamanho total: {totalSize / 1024:N0} KB");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Demonstra limitações de reflection em AOT
    /// </summary>
    static async Task DemoReflectionLimitations()
    {
        Console.WriteLine("🔍 Limitações de Reflection no AOT");
        Console.WriteLine("===================================");
        
        Console.WriteLine("✅ Operações permitidas:");
        
        // Type information básica (funciona)
        var type = typeof(DataPoint);
        Console.WriteLine($"  Tipo: {type.Name}");
        Console.WriteLine($"  Namespace: {type.Namespace}");
        Console.WriteLine($"  Assembly: {type.Assembly.GetName().Name}");
        
        // Propriedades conhecidas em tempo de compilação (funciona)
        var properties = type.GetProperties();
        Console.WriteLine($"  Propriedades: {string.Join(", ", properties.Select(p => p.Name))}");
        
        // Criação de instância (funciona se o tipo for conhecido)
        var instance = Activator.CreateInstance<DataPoint>();
        instance.Id = 42;
        instance.Value = 3.14;
        Console.WriteLine($"  Instância criada: Id={instance.Id}, Value={instance.Value}");
        
        Console.WriteLine("\n❌ Operações limitadas/não funcionais:");
        Console.WriteLine("  • Assembly.LoadFrom() - Carregamento dinâmico de assemblies");
        Console.WriteLine("  • Type.GetType(string) - Resolução de tipos por nome string");
        Console.WriteLine("  • Emit APIs - Geração de código em runtime");
        Console.WriteLine("  • Reflection sobre tipos não referenciados estaticamente");
        
        // Demonstrar alternativas AOT-friendly
        await DemoAOTFriendlyAlternatives();
        
        Console.WriteLine();
    }

    static async Task DemoAOTFriendlyAlternatives()
    {
        Console.WriteLine("\n✅ Alternativas AOT-friendly:");
        
        // 1. Source Generators ao invés de reflection
        Console.WriteLine("  1. Source Generators para geração de código");
        
        // 2. Interfaces e polimorfismo ao invés de reflection
        Console.WriteLine("  2. Interfaces ao invés de reflection dinâmica:");
        
        IProcessor[] processors = [
            new NumberProcessor(),
            new StringProcessor(),
            new DateProcessor()
        ];
        
        foreach (var processor in processors)
        {
            var result = processor.Process("Sample data");
            Console.WriteLine($"     {processor.GetType().Name}: {result}");
        }
        
        // 3. Delegates e Func<> ao invés de MethodInfo.Invoke
        Console.WriteLine("  3. Delegates ao invés de MethodInfo.Invoke:");
        
        var operations = new Dictionary<string, Func<double, double, double>>
        {
            ["add"] = (a, b) => a + b,
            ["multiply"] = (a, b) => a * b,
            ["power"] = Math.Pow
        };
        
        foreach (var (name, operation) in operations)
        {
            var result = operation(5, 3);
            Console.WriteLine($"     {name}(5, 3) = {result}");
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Mostra melhores práticas para Native AOT
    /// </summary>
    static void ShowBestPractices()
    {
        Console.WriteLine("💡 Melhores Práticas para Native AOT");
        Console.WriteLine("====================================");
        
        Console.WriteLine("🎯 Configuração do projeto:");
        Console.WriteLine("  • <PublishAot>true</PublishAot>");
        Console.WriteLine("  • <InvariantGlobalization>true</InvariantGlobalization>");
        Console.WriteLine("  • <StripSymbols>true</StripSymbols>");
        Console.WriteLine("  • <TrimMode>link</TrimMode>");
        Console.WriteLine();
        
        Console.WriteLine("🚀 Otimizações recomendadas:");
        Console.WriteLine("  • Use Source Generators ao invés de reflection");
        Console.WriteLine("  • Prefira interfaces e polimorfismo");
        Console.WriteLine("  • Evite Assembly.LoadFrom() e Type.GetType()");
        Console.WriteLine("  • Use JsonSerializerOptions com Source Generators");
        Console.WriteLine("  • Teste trim warnings com 'dotnet publish'");
        Console.WriteLine();
        
        Console.WriteLine("📊 Cenários ideais:");
        Console.WriteLine("  • ✅ APIs web simples e microsserviços");
        Console.WriteLine("  • ✅ Aplicações console e CLI tools");
        Console.WriteLine("  • ✅ Containers e serverless functions");
        Console.WriteLine("  • ✅ IoT e aplicações embedded");
        Console.WriteLine("  • ✅ Aplicações que precisam de startup rápido");
        Console.WriteLine();
        
        Console.WriteLine("⚠️ Cenários a evitar:");
        Console.WriteLine("  • ❌ Aplicações que dependem muito de reflection");
        Console.WriteLine("  • ❌ Plugins e sistemas com carregamento dinâmico");
        Console.WriteLine("  • ❌ Aplicações com muitas dependências não-AOT-ready");
        Console.WriteLine("  • ❌ Sistemas que usam Assembly.LoadFrom()");
        Console.WriteLine();
        
        Console.WriteLine("🔧 Comandos úteis:");
        Console.WriteLine("  # Publicar com AOT");
        Console.WriteLine("  dotnet publish -c Release");
        Console.WriteLine("  ");
        Console.WriteLine("  # Publicar para plataforma específica");
        Console.WriteLine("  dotnet publish -c Release -r win-x64");
        Console.WriteLine("  dotnet publish -c Release -r linux-x64");
        Console.WriteLine("  dotnet publish -c Release -r osx-arm64");
        Console.WriteLine("  ");
        Console.WriteLine("  # Analisar trim warnings");
        Console.WriteLine("  dotnet publish -c Release --verbosity normal");
        Console.WriteLine();
        
        ShowComparisonMetrics();
    }

    static void ShowComparisonMetrics()
    {
        Console.WriteLine("📈 Comparação típica: Normal vs AOT");
        Console.WriteLine("===================================");
        Console.WriteLine("Métrica               | Normal    | AOT       | Melhoria");
        Console.WriteLine("---------------------|-----------|-----------|----------");
        Console.WriteLine("Cold start           | 200-500ms | 5-20ms    | 10-50x  ");
        Console.WriteLine("Uso de memória       | 20-50MB   | 5-15MB    | 2-4x    ");
        Console.WriteLine("Tamanho runtime      | 150MB+    | 0MB       | ∞       ");
        Console.WriteLine("Tamanho executável   | 1-5MB     | 10-50MB   | -5-10x  ");
        Console.WriteLine("Tempo de JIT         | 50-200ms  | 0ms       | ∞       ");
        Console.WriteLine("Proteção código      | Baixa     | Alta      | +++     ");
        Console.WriteLine();
    }
}

// Modelos de dados para demonstrações
public record DataPoint
{
    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
}

public record ProcessedData
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Average { get; set; }
    public double Max { get; set; }
}

// Modelos para JSON (AOT-friendly)
public record ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserData? Data { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public record UserData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

// Interfaces para demonstrar alternativas ao reflection
public interface IProcessor
{
    string Process(string input);
}

public class NumberProcessor : IProcessor
{
    public string Process(string input) => $"Numbers: {input.Length}";
}

public class StringProcessor : IProcessor
{
    public string Process(string input) => $"Uppercase: {input.ToUpper()}";
}

public class DateProcessor : IProcessor
{
    public string Process(string input) => $"Processed at: {DateTime.Now:HH:mm:ss}";
}
